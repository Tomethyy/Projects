using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Application.Compliance;
using ShiftEngine.Compliance.ArbZG;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class ComplianceController(AppDbContext db) : ControllerBase
{
    [HttpGet("evaluate/{periodId:guid}")]
    public async Task<ActionResult<List<ComplianceFinding>>> Evaluate(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var assigns = await db.ShiftAssignments.Include(a => a.ShiftTier).Include(a => a.Employee)
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId).ToListAsync(ct);
        var leaves = await db.LeaveRecords.Where(l => l.TenantId == tenantId).ToListAsync(ct);
        var year = assigns.Select(a => a.WorkDate.Year).DefaultIfEmpty(DateTime.UtcNow.Year).Max();
        var ctx = new RosterEvaluationContext
        {
            TenantId = tenantId,
            Assignments = assigns,
            Leaves = leaves,
            PlanningYear = year
        };
        var findings = ArbZGRuleRegistry.DefaultRules.SelectMany(r => r.Evaluate(ctx)).ToList();
        return findings;
    }

    [HttpGet("bv-audit/{periodId:guid}")]
    public async Task<ActionResult<IReadOnlyList<BvAuditFinding>>> BvAudit(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var exists = await db.RosterPeriods.AnyAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (!exists) return NotFound();
        var year = await db.ShiftAssignments.Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId)
            .Select(a => a.WorkDate.Year).DefaultIfEmpty(DateTime.UtcNow.Year).MaxAsync(ct);
        return Ok(BvAuditCatalog.DefaultChecklist(year));
    }
}
