using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftEngine.Domain;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = SecurityRoles.OperationsReaders)]
public class PdfArchiveController : ControllerBase
{
    [HttpGet("roster/{periodId:guid}")]
    public ActionResult RosterPdf(Guid periodId)
    {
        var pdf = "%PDF-1.4\n1 0 obj<<>>endobj\ntrailer<<>>\n%%EOF\n"u8.ToArray();
        return File(pdf, "application/pdf", $"roster-{periodId}.pdf");
    }
}
