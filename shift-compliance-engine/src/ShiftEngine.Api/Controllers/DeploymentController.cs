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
public class DeploymentController(AppDbContext db) : ControllerBase
{
    [HttpGet("posts")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<List<DeploymentPost>> ListPosts(CancellationToken ct) =>
        await db.DeploymentPosts.Where(p => p.TenantId == User.GetTenantId()).AsNoTracking().ToListAsync(ct);

    [HttpPost("posts")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<Guid>> CreatePost([FromBody] DeploymentPost post, CancellationToken ct)
    {
        post.Id = Guid.NewGuid();
        post.TenantId = User.GetTenantId();
        db.DeploymentPosts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }
}
