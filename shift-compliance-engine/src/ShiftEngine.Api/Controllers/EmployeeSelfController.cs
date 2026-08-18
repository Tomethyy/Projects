using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/employee")]
[Authorize(Roles = $"{SecurityRoles.Admin},{SecurityRoles.Employee}")]
public class EmployeeSelfController(AppDbContext db) : ControllerBase
{
    [HttpGet("roster")]
    public async Task<ActionResult<object>> MyRoster([FromQuery] int year, [FromQuery] int month, CancellationToken ct)
    {
        var uid = User.GetUserId();
        var emp = await db.Employees.FirstOrDefaultAsync(e => e.TenantId == User.GetTenantId() && e.LinkedUserId == uid, ct);
        if (emp == null)
            return Ok(new { assignments = Array.Empty<object>(), publishedOnly = true, message = "Link employee to user" });

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var list = await db.ShiftAssignments
            .Include(a => a.ShiftTier)
            .Include(a => a.RosterPeriod)
            .Where(a =>
                a.TenantId == User.GetTenantId() &&
                a.EmployeeId == emp.Id &&
                a.WorkDate >= start &&
                a.WorkDate <= end &&
                a.RosterPeriod.IsPublished)
            .Select(a => new { a.WorkDate, tier = a.ShiftTier.DisplayName, a.ShiftTier.Code })
            .ToListAsync(ct);

        var draftExists = await db.ShiftAssignments
            .AnyAsync(a =>
                a.TenantId == User.GetTenantId() &&
                a.EmployeeId == emp.Id &&
                a.WorkDate >= start &&
                a.WorkDate <= end &&
                !a.RosterPeriod.IsPublished, ct);

        return Ok(new
        {
            assignments = list,
            publishedOnly = true,
            draftExists,
            message = list.Count == 0 && draftExists
                ? "Draft roster exists but is not published yet."
                : null
        });
    }
}
