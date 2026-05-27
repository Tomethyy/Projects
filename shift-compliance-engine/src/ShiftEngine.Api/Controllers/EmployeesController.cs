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
public class EmployeesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<Employee>>> List(CancellationToken ct) =>
        await db.Employees.Where(e => e.TenantId == User.GetTenantId()).AsNoTracking().ToListAsync(ct);

    [HttpPost]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<Guid>> Create([FromBody] Employee body, CancellationToken ct)
    {
        body.Id = Guid.NewGuid();
        body.TenantId = User.GetTenantId();
        db.Employees.Add(body);
        await db.SaveChangesAsync(ct);
        return body.Id;
    }
}
