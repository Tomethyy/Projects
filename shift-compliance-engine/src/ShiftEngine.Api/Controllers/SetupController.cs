using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Identity;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SetupController(UserManager<AppUser> users, AppDbContext db) : ControllerBase
{
    public record WizardRequest(
        string TenantSlug,
        string TenantDisplayName,
        string DefaultLocale,
        string AdminEmail,
        string AdminPassword,
        string AdminDisplayName,
        bool EnableAiKeyPlaceholder,
        string? SmtpHost,
        int? SmtpPort,
        string? SmtpUsername,
        string? SmtpPassword,
        string? SmtpFromEmail,
        string? AiApiKey,
        /// <summary>
        /// Optional lines: <c>PersonnelNumber;DisplayName;Email</c> (email optional).
        /// First line may be header <c>PersonnelNumber;DisplayName;Email</c>.
        /// </summary>
        string? EmployeeInviteCsv);

    private sealed record InviteRow(string PersonnelNumber, string DisplayName, string? Email);

    [HttpPost("wizard")]
    public async Task<ActionResult<object>> CompleteWizard([FromBody] WizardRequest req, CancellationToken ct)
    {
        if (await db.Tenants.AnyAsync(ct)) return Conflict("Already initialized");

        var adminEmailNorm = req.AdminEmail.Trim().ToUpperInvariant();
        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = req.TenantSlug.Trim(),
            DisplayName = req.TenantDisplayName.Trim(),
            DefaultLocale = string.IsNullOrWhiteSpace(req.DefaultLocale) ? "de-DE" : req.DefaultLocale.Trim(),
            BundeslandCode = "DE-SN",
            SmtpHost = string.IsNullOrWhiteSpace(req.SmtpHost) ? null : req.SmtpHost.Trim(),
            SmtpPort = req.SmtpPort,
            SmtpUsername = string.IsNullOrWhiteSpace(req.SmtpUsername) ? null : req.SmtpUsername.Trim(),
            SmtpPassword = string.IsNullOrWhiteSpace(req.SmtpPassword) ? null : req.SmtpPassword,
            SmtpFromEmail = string.IsNullOrWhiteSpace(req.SmtpFromEmail) ? null : req.SmtpFromEmail.Trim(),
            AiApiKeySecret = string.IsNullOrWhiteSpace(req.AiApiKey)
                ? (req.EnableAiKeyPlaceholder ? "PLACEHOLDER" : null)
                : req.AiApiKey.Trim()
        };

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync(ct);

        var user = new AppUser
        {
            UserName = req.AdminEmail.Trim(),
            Email = req.AdminEmail.Trim(),
            TenantId = tenant.Id,
            DisplayName = req.AdminDisplayName.Trim(),
            EmailConfirmed = true
        };
        var res = await users.CreateAsync(user, req.AdminPassword);
        if (!res.Succeeded)
        {
            await tx.RollbackAsync(ct);
            return BadRequest(res.Errors);
        }

        await users.AddToRoleAsync(user, SecurityRoles.Admin);

        db.ShiftTiers.AddRange(
            new ShiftTier { Id = Guid.NewGuid(), TenantId = tenant.Id, Code = "EARLY", DisplayName = "Früh", StartLocal = new TimeOnly(6, 0), EndLocal = new TimeOnly(14, 0) },
            new ShiftTier { Id = Guid.NewGuid(), TenantId = tenant.Id, Code = "LATE", DisplayName = "Spät", StartLocal = new TimeOnly(14, 0), EndLocal = new TimeOnly(22, 0) },
            new ShiftTier { Id = Guid.NewGuid(), TenantId = tenant.Id, Code = "NIGHT", DisplayName = "Nacht", StartLocal = new TimeOnly(22, 0), EndLocal = new TimeOnly(6, 0), IsNight = true }
        );

        var invitations = new List<object>();
        var linkedAdmin = new List<string>();
        var skipped = new List<string>();
        var seenPersonnel = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var toInvite = new List<(Employee Emp, string Email)>();

        foreach (var row in ParseInviteCsv(req.EmployeeInviteCsv))
        {
            if (string.IsNullOrWhiteSpace(row.PersonnelNumber))
            {
                skipped.Add("empty-personnel");
                continue;
            }

            var pn = row.PersonnelNumber.Trim();
            if (!seenPersonnel.Add(pn))
            {
                skipped.Add($"duplicate-personnel:{pn}");
                continue;
            }

            var emp = new Employee
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                PersonnelNumber = pn,
                DisplayName = string.IsNullOrWhiteSpace(row.DisplayName) ? pn : row.DisplayName.Trim(),
                IsActive = true
            };

            var email = string.IsNullOrWhiteSpace(row.Email) ? null : row.Email.Trim();
            if (email is not null && email.ToUpperInvariant() == adminEmailNorm)
            {
                emp.LinkedUserId = user.Id;
                linkedAdmin.Add(pn);
            }
            else if (email is not null)
            {
                toInvite.Add((emp, email));
            }

            db.Employees.Add(emp);
        }

        await db.SaveChangesAsync(ct);

        foreach (var (emp, email) in toInvite)
        {
            var temp = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)).TrimEnd('=');
            var u = new AppUser
            {
                UserName = email,
                Email = email,
                TenantId = tenant.Id,
                DisplayName = emp.DisplayName,
                EmailConfirmed = true
            };
            var ir = await users.CreateAsync(u, temp);
            if (!ir.Succeeded)
            {
                skipped.Add($"user-failed:{emp.PersonnelNumber}:{string.Join(',', ir.Errors.Select(e => e.Description))}");
                continue;
            }

            await users.AddToRoleAsync(u, SecurityRoles.Employee);
            emp.LinkedUserId = u.Id;
            invitations.Add(new { email, temporaryPassword = temp, personnelNumber = emp.PersonnelNumber });
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            tenantId = tenant.Id,
            slug = tenant.Slug,
            invitations,
            linkedAdminPersonnelNumbers = linkedAdmin,
            skipped
        });
    }

    private static IEnumerable<InviteRow> ParseInviteCsv(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv)) yield break;
        var lines = csv.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var start = 0;
        if (lines.Length > 0 && lines[0].Contains("PersonnelNumber", StringComparison.OrdinalIgnoreCase))
            start = 1;

        foreach (var line in lines.Skip(start))
        {
            var parts = line.Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length < 2) continue;
            yield return new InviteRow(parts[0], parts[1], parts.Length > 2 ? parts[2] : null);
        }
    }
}
