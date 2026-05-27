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
public class TrainingController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<List<TrainingEvent>> List(CancellationToken ct) =>
        await db.TrainingEvents.Where(t => t.TenantId == User.GetTenantId()).AsNoTracking().ToListAsync(ct);

    [HttpPost]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<Guid> Upsert([FromBody] TrainingEvent ev, CancellationToken ct)
    {
        ev.TenantId = User.GetTenantId();
        if (ev.Id == Guid.Empty) ev.Id = Guid.NewGuid();
        var existing = await db.TrainingEvents.AnyAsync(t => t.Id == ev.Id && t.TenantId == ev.TenantId, ct);
        if (existing) db.TrainingEvents.Update(ev);
        else db.TrainingEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        return ev.Id;
    }
}
