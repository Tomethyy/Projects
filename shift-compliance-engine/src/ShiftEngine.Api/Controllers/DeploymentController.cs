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
        if (post.IsGenderIrrelevant)
        {
            post.MinRequiredFemale = 0;
            post.MinRequiredMale = 0;
        }
        db.DeploymentPosts.Add(post);
        await db.SaveChangesAsync(ct);
        return post.Id;
    }

    [HttpPut("posts/{id:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] DeploymentPost body, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var post = await db.DeploymentPosts.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);
        if (post is null) return NotFound();
        post.Name = body.Name.Trim();
        post.WindowStart = body.WindowStart;
        post.WindowEnd = body.WindowEnd;
        post.RequiredHeadcount = body.RequiredHeadcount;
        post.IsGenderIrrelevant = body.IsGenderIrrelevant;
        post.MinRequiredFemale = body.IsGenderIrrelevant ? 0 : body.MinRequiredFemale;
        post.MinRequiredMale = body.IsGenderIrrelevant ? 0 : body.MinRequiredMale;
        post.RequiredQualificationCode = body.RequiredQualificationCode;
        post.BufferPercent = body.BufferPercent;
        post.AllowedRolesCsv = body.AllowedRolesCsv;
        post.StandardSecurityWeight = body.StandardSecurityWeight;
        post.LskpWeightOnSecurityPost = body.LskpWeightOnSecurityPost;
        await db.SaveChangesAsync(ct);
        return Ok(post);
    }

    [HttpDelete("posts/{id:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> DeletePost(Guid id, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var post = await db.DeploymentPosts.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id, ct);
        if (post is null) return NotFound();
        db.DeploymentPosts.Remove(post);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }
}
