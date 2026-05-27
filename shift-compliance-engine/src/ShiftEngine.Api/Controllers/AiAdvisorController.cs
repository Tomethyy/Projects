using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Domain;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsWriters)]
public class AiAdvisorController : ControllerBase
{
    public record AnalyzeRequest(string PromptSnippet);

    [HttpPost("analyze")]
    public ActionResult<object> Analyze([FromBody] AnalyzeRequest req) =>
        Ok(new { summary = "AI integration placeholder (Tier 4).", tenant = User.GetTenantId(), promptLength = req.PromptSnippet.Length });
}
