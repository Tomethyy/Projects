using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Application.Compliance;
using ShiftEngine.Compliance.ArbZG;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class ComplianceController(AppDbContext db) : ControllerBase
{
    private async Task<(List<ShiftAssignment> Assignments, List<LeaveRecord> Leaves, List<Employee> Employees, RosterPeriod? Period)> LoadPeriodAsync(
        Guid tenantId, Guid periodId, CancellationToken ct)
    {
        var period = await db.RosterPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (period is null) return ([], [], [], null);
        var assigns = await db.ShiftAssignments
            .Include(a => a.ShiftTier)
            .Include(a => a.Employee)
            .Include(a => a.DeploymentPost)
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId)
            .ToListAsync(ct);
        var leaves = await db.LeaveRecords.Where(l => l.TenantId == tenantId).ToListAsync(ct);
        var employees = await db.Employees
            .Include(e => e.Qualifications).ThenInclude(q => q.Qualification)
            .Where(e => e.TenantId == tenantId && e.IsActive)
            .ToListAsync(ct);
        return (assigns, leaves, employees, period);
    }

    [HttpGet("evaluate/{periodId:guid}")]
    public async Task<ActionResult<List<ComplianceFinding>>> Evaluate(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var (assigns, leaves, _, period) = await LoadPeriodAsync(tenantId, periodId, ct);
        if (period is null) return NotFound();
        var year = assigns.Count > 0 ? assigns.Max(a => a.WorkDate.Year) : period.StartDate.Year;
        var ctx = new RosterEvaluationContext { TenantId = tenantId, Assignments = assigns, Leaves = leaves, PlanningYear = year };
        return ArbZGRuleRegistry.DefaultRules.SelectMany(r => r.Evaluate(ctx)).ToList();
    }

    [HttpGet("bv-audit/{periodId:guid}")]
    public async Task<ActionResult<IReadOnlyList<BvAuditFinding>>> BvAudit(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var period = await db.RosterPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (period is null) return NotFound();
        return Ok(BvAuditCatalog.DefaultChecklist(period.StartDate.Year));
    }

    [HttpPost("propose-fixes/{periodId:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<ComplianceFixProposal>> ProposeFixes(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var (assigns, leaves, employees, period) = await LoadPeriodAsync(tenantId, periodId, ct);
        if (period is null) return NotFound();
        return ComplianceRemediator.Propose(assigns, leaves, employees);
    }

    public record ApplyFixItem(string Action, Guid AssignmentId, Guid? TargetEmployeeId);

    public record ApplyFixesRequest(
        IReadOnlyList<Guid>? AssignmentIds,
        IReadOnlyList<ApplyFixItem>? Fixes);

    [HttpPost("apply-fixes/{periodId:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<ComplianceFixProposal>> ApplyFixes(Guid periodId, [FromBody] ApplyFixesRequest? req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var period = await db.RosterPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (period is null) return NotFound();
        if (period.IsPublished) return Conflict("Published periods cannot be auto-fixed.");

        var fixes = req?.Fixes?.ToList();
        if (fixes is null || fixes.Count == 0)
        {
            if (req?.AssignmentIds is { Count: > 0 })
            {
                fixes = req.AssignmentIds
                    .Select(id => new ApplyFixItem(ComplianceRemediator.RemoveAssignment, id, null))
                    .ToList();
            }
            else
            {
                var (assigns, leaves, employees, _) = await LoadPeriodAsync(tenantId, periodId, ct);
                var proposal = ComplianceRemediator.Propose(assigns, leaves, employees);
                fixes = proposal.Actions
                    .Select(a => new ApplyFixItem(a.Action, a.AssignmentId, a.TargetEmployeeId))
                    .ToList();
            }
        }

        if (fixes.Count > 0)
        {
            var assignmentIds = fixes.Select(f => f.AssignmentId).Distinct().ToList();
            var assignments = await db.ShiftAssignments
                .Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId && assignmentIds.Contains(a.Id))
                .ToListAsync(ct);
            var byId = assignments.ToDictionary(a => a.Id);

            foreach (var fix in fixes)
            {
                if (!byId.TryGetValue(fix.AssignmentId, out var assignment)) continue;
                if (fix.Action == ComplianceRemediator.ReassignAssignment && fix.TargetEmployeeId.HasValue)
                {
                    var targetOk = await db.Employees.AnyAsync(
                        e => e.TenantId == tenantId && e.Id == fix.TargetEmployeeId && e.IsActive, ct);
                    if (!targetOk) continue;
                    assignment.EmployeeId = fix.TargetEmployeeId.Value;
                }
                else
                    db.ShiftAssignments.Remove(assignment);
            }

            await db.SaveChangesAsync(ct);
        }

        var (afterAssigns, afterLeaves, afterEmployees, _) = await LoadPeriodAsync(tenantId, periodId, ct);
        return ComplianceRemediator.Propose(afterAssigns, afterLeaves, afterEmployees);
    }
}
