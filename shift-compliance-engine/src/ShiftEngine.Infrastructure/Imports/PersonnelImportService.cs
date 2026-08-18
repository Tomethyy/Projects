using Microsoft.EntityFrameworkCore;
using ShiftEngine.Application.Imports;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Infrastructure.Imports;

public sealed class PersonnelImportService(AppDbContext db)
{
    public async Task<PersonnelImportResult> ImportPersonnelAsync(Guid tenantId, string csv, bool deactivateMissing, CancellationToken ct)
    {
        var parsed = PersonnelFileParser.Parse(csv);
        var errors = parsed.Where(r => r.Error is not null).ToList();
        if (errors.Count > 0)
            return new PersonnelImportResult(parsed.Count, 0, 0, 0, errors.Select(e => $"Line {e.LineNumber}: {e.Error}").ToList());

        var existing = await db.Employees.Where(e => e.TenantId == tenantId).ToListAsync(ct);
        var byPn = existing.ToDictionary(e => e.PersonnelNumber, StringComparer.OrdinalIgnoreCase);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var created = 0;
        var updated = 0;

        foreach (var row in parsed.Where(r => r.Error is null))
        {
            if (!seen.Add(row.PersonnelNumber))
                return new PersonnelImportResult(parsed.Count, created, updated, 0,
                    [$"Duplicate PersonnelNumber in file: {row.PersonnelNumber}"]);

            if (byPn.TryGetValue(row.PersonnelNumber, out var emp))
            {
                emp.DisplayName = row.DisplayName;
                emp.ContractedHoursMonthly = row.ContractedHoursMonthly;
                emp.GenderCode = row.GenderCode;
                emp.PrimaryRole = row.PrimaryRole;
                emp.ExternalLegacyId = row.ExternalLegacyId;
                emp.IsActive = true;
                updated++;
            }
            else
            {
                db.Employees.Add(new Employee
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    PersonnelNumber = row.PersonnelNumber,
                    DisplayName = row.DisplayName,
                    ContractedHoursMonthly = row.ContractedHoursMonthly,
                    GenderCode = row.GenderCode,
                    PrimaryRole = row.PrimaryRole,
                    ExternalLegacyId = row.ExternalLegacyId,
                    IsActive = true
                });
                created++;
            }
        }

        var deactivated = 0;
        if (deactivateMissing)
        {
            foreach (var emp in existing.Where(e => e.IsActive && !seen.Contains(e.PersonnelNumber)))
            {
                emp.IsActive = false;
                deactivated++;
            }
        }

        await db.SaveChangesAsync(ct);
        return new PersonnelImportResult(parsed.Count, created, updated, deactivated, []);
    }

    public async Task<PositionImportResult> ImportPositionsAsync(Guid tenantId, string csv, bool replaceAll, CancellationToken ct)
    {
        var parsed = PositionFileParser.Parse(csv);
        var errors = parsed.Where(r => r.Error is not null).ToList();
        if (errors.Count > 0)
            return new PositionImportResult(parsed.Count, 0, 0, errors.Select(e => $"Line {e.LineNumber}: {e.Error}").ToList());

        if (replaceAll)
        {
            var old = await db.DeploymentPosts.Where(p => p.TenantId == tenantId).ToListAsync(ct);
            db.DeploymentPosts.RemoveRange(old);
        }

        var created = 0;
        foreach (var row in parsed.Where(r => r.Error is null))
        {
            db.DeploymentPosts.Add(new DeploymentPost
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = row.Name,
                WindowStart = row.WindowStart,
                WindowEnd = row.WindowEnd,
                RequiredHeadcount = row.RequiredHeadcount,
                MinRequiredFemale = row.IsGenderIrrelevant ? 0 : row.MinRequiredFemale,
                MinRequiredMale = row.IsGenderIrrelevant ? 0 : row.MinRequiredMale,
                IsGenderIrrelevant = row.IsGenderIrrelevant,
                RequiredQualificationCode = row.RequiredQualificationCode,
                BufferPercent = row.BufferPercent
            });
            created++;
        }

        await db.SaveChangesAsync(ct);
        return new PositionImportResult(parsed.Count, created, 0, []);
    }
}

public sealed record PersonnelImportResult(
    int RowCount,
    int Created,
    int Updated,
    int Deactivated,
    IReadOnlyList<string> Errors);

public sealed record PositionImportResult(
    int RowCount,
    int Created,
    int Replaced,
    IReadOnlyList<string> Errors);
