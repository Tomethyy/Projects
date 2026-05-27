using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Domain.Rostering;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RosterController(AppDbContext db) : ControllerBase
{
    public record GenerateYearRequest(
        Guid EmployeeId,
        int Year,
        RosterPatternKind Pattern,
        DateOnly AnchorFirstWorkDay,
        Guid? ShiftTierId,
        string? LegacySource);

    public record PublishRequest(Guid RosterPeriodId);

    [HttpPost("periods")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<Guid>> CreatePeriod([FromBody] RosterPeriod period, CancellationToken ct)
    {
        period.Id = Guid.NewGuid();
        period.TenantId = User.GetTenantId();
        period.IsPublished = false;
        db.RosterPeriods.Add(period);
        await db.SaveChangesAsync(ct);
        return period.Id;
    }

    [HttpPost("generate-year")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult> GenerateYear([FromBody] GenerateYearRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var emp = await db.Employees.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (emp == null) return NotFound();
        var tierId = req.ShiftTierId ?? await db.ShiftTiers.Where(t => t.TenantId == tenantId && t.Code == "EARLY").Select(t => t.Id).FirstAsync(ct);
        var tier = await db.ShiftTiers.FirstAsync(t => t.Id == tierId, ct);
        var start = new DateOnly(req.Year, 1, 1);
        var end = new DateOnly(req.Year, 12, 31);
        var period = new RosterPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = $"{req.Year} {emp.DisplayName}",
            StartDate = start,
            EndDate = end,
            LegacySource = string.IsNullOrWhiteSpace(req.LegacySource) ? "Synthetic" : req.LegacySource.Trim(),
            LegacyReferenceMode = LegacyReferenceMode.ShadowPlanning
        };
        db.RosterPeriods.Add(period);
        foreach (var (d, work) in RosterRotationPlanner.EnumerateYear(start, end, req.Pattern, emp.ContractedHoursMonthly, req.AnchorFirstWorkDay))
        {
            if (!work) continue;
            db.ShiftAssignments.Add(new ShiftAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RosterPeriodId = period.Id,
                EmployeeId = emp.Id,
                ShiftTierId = tier.Id,
                WorkDate = d
            });
        }

        await db.SaveChangesAsync(ct);
        return Ok(new { period.Id });
    }

    [HttpPost("publish")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult> Publish([FromBody] PublishRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var p = await db.RosterPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == req.RosterPeriodId, ct);
        if (p == null) return NotFound();
        p.IsPublished = true;
        p.PublishedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpGet("assignments")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<ShiftAssignment>>> Assignments([FromQuery] Guid periodId, CancellationToken ct) =>
        await db.ShiftAssignments.Include(a => a.Employee).Include(a => a.ShiftTier)
            .Where(a => a.TenantId == User.GetTenantId() && a.RosterPeriodId == periodId)
            .AsNoTracking().ToListAsync(ct);

    [HttpGet("shift-tiers")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<ShiftTier>>> ShiftTiers(CancellationToken ct) =>
        await db.ShiftTiers.Where(t => t.TenantId == User.GetTenantId()).OrderBy(t => t.Code).AsNoTracking().ToListAsync(ct);

    public record PatchAssignmentTierRequest(Guid ShiftTierId);

    [HttpPatch("assignments/{assignmentId:guid}/tier")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> PatchAssignmentTier(Guid assignmentId, [FromBody] PatchAssignmentTierRequest body, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var a = await db.ShiftAssignments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assignmentId, ct);
        if (a == null) return NotFound();
        var tierOk = await db.ShiftTiers.AnyAsync(t => t.TenantId == tenantId && t.Id == body.ShiftTierId, ct);
        if (!tierOk) return BadRequest("Unknown shift tier.");
        a.ShiftTierId = body.ShiftTierId;
        await db.SaveChangesAsync(ct);
        return Ok();
    }
}
