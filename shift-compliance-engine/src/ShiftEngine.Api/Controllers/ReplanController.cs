using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Domain;
using ShiftEngine.Infrastructure.SickLeave;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsWriters)]
public class ReplanController(SickLeaveReplanService svc) : ControllerBase
{
    [HttpPost("propose/{ledgerEntryId:guid}")]
    public async Task<IActionResult> Propose(Guid ledgerEntryId, CancellationToken ct) =>
        Ok(await svc.ProposeAsync(User.GetTenantId(), ledgerEntryId, ct));

    public record ApplyRequest(Guid ProposalId, Guid ReplacementEmployeeId);

    [HttpPost("apply")]
    public async Task<IActionResult> Apply([FromBody] ApplyRequest req, CancellationToken ct)
    {
        await svc.ApplyAsync(User.GetTenantId(), req.ProposalId, req.ReplacementEmployeeId, ct);
        return Ok();
    }
}
