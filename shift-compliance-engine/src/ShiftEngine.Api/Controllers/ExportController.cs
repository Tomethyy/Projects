using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Domain;
using ShiftEngine.Infrastructure.Export;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class ExportController(RosterExcelExportService export) : ControllerBase
{
    [HttpGet("roster/{periodId:guid}")]
    public async Task<IActionResult> RosterExcel(Guid periodId, CancellationToken ct)
    {
        var bytes = await export.ExportRosterAsync(User.GetTenantId(), periodId, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"roster-{periodId}.xlsx");
    }
}
