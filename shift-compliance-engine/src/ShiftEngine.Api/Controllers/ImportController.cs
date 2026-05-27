using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Application.Imports;
using ShiftEngine.Domain;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsWriters)]
public class ImportController : ControllerBase
{
    [HttpPost("secplan/dry-run")]
    public async Task<ActionResult<SecPlanImportResult>> SecPlanDryRun(IFormFile file, CancellationToken ct)
    {
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;
        return SecPlanExcelImporter.DryRun(ms);
    }
}
