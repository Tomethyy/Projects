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
public class LedgerController(AppDbContext db) : ControllerBase
{
    public record SickCallRequest(Guid? ShiftAssignmentId, Guid EmployeeId, DateOnly Date, LedgerEntryKind Kind, string? Notes);

    [HttpPost("sick-or-callout")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> Record([FromBody] SickCallRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var entry = new DailyLedgerEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntryDate = req.Date,
            Kind = req.Kind,
            EmployeeId = req.EmployeeId,
            ShiftAssignmentId = req.ShiftAssignmentId,
            Notes = req.Notes
        };
        db.DailyLedgerEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return Ok(new { id = entry.Id });
    }

    [HttpGet("today")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<DailyLedgerEntry>>> Today([FromQuery] DateOnly? date, CancellationToken ct)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.DailyLedgerEntries.Include(e => e.Employee)
            .Where(e => e.TenantId == User.GetTenantId() && e.EntryDate == d)
            .AsNoTracking().ToListAsync(ct);
    }
}
