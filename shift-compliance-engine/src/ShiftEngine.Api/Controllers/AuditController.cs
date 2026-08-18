using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class AuditController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AuditLogEntry>>> List([FromQuery] int limit = 50, CancellationToken ct = default) =>
        await db.AuditLogs
            .Where(a => a.TenantId == User.GetTenantId())
            .OrderByDescending(a => a.CreatedAt)
            .Take(Math.Clamp(limit, 1, 200))
            .AsNoTracking()
            .ToListAsync(ct);
}
