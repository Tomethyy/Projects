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
public class SwapsController(AppDbContext db) : ControllerBase
{
    public record SwapRequest(Guid RequesterAssignmentId, Guid TargetAssignmentId, Guid RequesterEmployeeId, Guid TargetEmployeeId);

    [HttpPost("request")]
    public async Task<ActionResult<Guid>> CreateRequest([FromBody] SwapRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var swap = new ShiftSwap
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            RequesterAssignmentId = req.RequesterAssignmentId,
            TargetAssignmentId = req.TargetAssignmentId,
            RequesterEmployeeId = req.RequesterEmployeeId,
            TargetEmployeeId = req.TargetEmployeeId,
            Status = ShiftSwapStatus.Pending
        };
        db.ShiftSwaps.Add(swap);
        await db.SaveChangesAsync(ct);
        return swap.Id;
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = $"{SecurityRoles.Admin},{SecurityRoles.Manager},{SecurityRoles.Planner}")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var s = await db.ShiftSwaps.FirstOrDefaultAsync(x => x.TenantId == User.GetTenantId() && x.Id == id, ct);
        if (s == null) return NotFound();
        s.Status = ShiftSwapStatus.Approved;
        s.ManagerUserId = User.GetUserId();
        var a1 = await db.ShiftAssignments.FirstAsync(x => x.Id == s.RequesterAssignmentId, ct);
        var a2 = await db.ShiftAssignments.FirstAsync(x => x.Id == s.TargetAssignmentId, ct);
        (a1.EmployeeId, a2.EmployeeId) = (a2.EmployeeId, a1.EmployeeId);
        await db.SaveChangesAsync(ct);
        return Ok();
    }
}
