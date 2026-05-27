using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeaveController(AppDbContext db) : ControllerBase
{
    public record FreezeCarryoverRequest(int CarryoverYear, Guid? EmployeeId);

    [HttpPost]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<Guid>> Create([FromBody] LeaveRecord leave, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        if (leave.Source == LeaveSource.CarryoverLocked)
        {
            var dup = await db.LeaveRecords.AnyAsync(l =>
                l.TenantId == tenantId &&
                l.EmployeeId == leave.EmployeeId &&
                l.Source == LeaveSource.CarryoverLocked &&
                l.CarryoverYear == leave.CarryoverYear, ct);
            if (dup) return Conflict("Carryover line already exists for this employee and year.");
        }

        leave.Id = Guid.NewGuid();
        leave.TenantId = tenantId;
        db.LeaveRecords.Add(leave);
        await db.SaveChangesAsync(ct);
        return leave.Id;
    }

    [HttpGet("locked/{year:int}")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<List<LeaveRecord>> LockedCarryover(int year, CancellationToken ct) =>
        await db.LeaveRecords.Where(l =>
                l.TenantId == User.GetTenantId() &&
                l.Source == LeaveSource.CarryoverLocked &&
                l.CarryoverYear < year &&
                l.IsApproved)
            .AsNoTracking()
            .ToListAsync(ct);

    /// <summary>Locks carryover lines for payroll / year close (no further edits).</summary>
    [HttpPost("carryover/freeze")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> FreezeCarryover([FromBody] FreezeCarryoverRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var q = db.LeaveRecords.Where(l =>
            l.TenantId == tenantId &&
            l.Source == LeaveSource.CarryoverLocked &&
            l.CarryoverYear == req.CarryoverYear &&
            (req.EmployeeId == null || l.EmployeeId == req.EmployeeId));
        await q.ExecuteUpdateAsync(s => s.SetProperty(l => l.IsCarryoverFrozen, true), ct);
        return Ok();
    }
}
