using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Domain;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class AnalyticsController : ControllerBase
{
    [HttpGet("heatmap/{year:int}")]
    public ActionResult<object> Heatmap(int year) =>
        Ok(new { year, cells = Array.Empty<object>(), note = "Tier 4 conflict heatmap placeholder" });

    [HttpGet("buffer-mvp")]
    public ActionResult<object> BufferMvp() =>
        Ok(new
        {
            note = "Buffer consumption vs deployment headcount (MVP numbers follow in Phase 2b).",
            samplePosts = 0,
            consumedBufferHours = 0m
        });
}
