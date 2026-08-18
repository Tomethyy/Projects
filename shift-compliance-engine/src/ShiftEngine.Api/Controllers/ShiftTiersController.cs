using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/shift-tiers")]
[Authorize]
public class ShiftTiersController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<List<ShiftTier>> List(CancellationToken ct) =>
        await db.ShiftTiers.Where(t => t.TenantId == User.GetTenantId()).OrderBy(t => t.Code).AsNoTracking().ToListAsync(ct);

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ShiftTier body, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var tier = await db.ShiftTiers.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Id == id, ct);
        if (tier is null) return NotFound();
        tier.Code = body.Code.Trim();
        tier.DisplayName = body.DisplayName.Trim();
        tier.StartLocal = body.StartLocal;
        tier.EndLocal = body.EndLocal;
        tier.IsNight = body.IsNight;
        tier.IsSundayPremium = body.IsSundayPremium;
        await db.SaveChangesAsync(ct);
        return Ok(tier);
    }
}
