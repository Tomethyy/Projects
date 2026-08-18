using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Application.Imports;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Audit;
using ShiftEngine.Infrastructure.Imports;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PersonnelController(AppDbContext db, PersonnelImportService import, AuditLogService audit) : ControllerBase
{
    public record CsvImportRequest(string CsvText, bool DeactivateMissing = false, bool ReplaceAllPositions = true);

    [HttpGet("export")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<string>> ExportPersonnel(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var rows = await db.Employees
            .Where(e => e.TenantId == tenantId)
            .OrderBy(e => e.PersonnelNumber)
            .Select(e => new PersonnelFileRow(
                0,
                e.PersonnelNumber,
                e.DisplayName,
                e.ContractedHoursMonthly,
                e.GenderCode,
                e.PrimaryRole,
                null,
                e.ExternalLegacyId,
                null))
            .ToListAsync(ct);
        return Content(PersonnelFileParser.FormatExport(rows), "text/csv; charset=utf-8");
    }

    [HttpPost("import/dry-run")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public ActionResult<object> DryRunPersonnel([FromBody] CsvImportRequest req)
    {
        var rows = PersonnelFileParser.Parse(req.CsvText);
        return Ok(new
        {
            rowCount = rows.Count,
            valid = rows.Count(r => r.Error is null),
            errors = rows.Where(r => r.Error is not null).Select(r => new { r.LineNumber, r.Error })
        });
    }

    [HttpPost("import")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<PersonnelImportResult>> ImportPersonnel([FromBody] CsvImportRequest req, CancellationToken ct)
    {
        var result = await import.ImportPersonnelAsync(User.GetTenantId(), req.CsvText, req.DeactivateMissing, ct);
        if (result.Errors.Count > 0) return BadRequest(result);
        await audit.WriteAsync(
            User.GetTenantId(),
            User.GetUserId(),
            "Personnel.Import",
            "Employee",
            null,
            new { result.Created, result.Updated, result.Deactivated, result.RowCount },
            ct);
        return result;
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] Employee body, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var emp = await db.Employees.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id, ct);
        if (emp is null) return NotFound();
        emp.PersonnelNumber = body.PersonnelNumber.Trim();
        emp.DisplayName = body.DisplayName.Trim();
        emp.ContractedHoursMonthly = body.ContractedHoursMonthly;
        emp.GenderCode = body.GenderCode;
        emp.PrimaryRole = body.PrimaryRole;
        emp.ExternalLegacyId = body.ExternalLegacyId;
        emp.IsActive = body.IsActive;
        await db.SaveChangesAsync(ct);
        return Ok(emp);
    }

    [HttpGet("positions/export")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<string>> ExportPositions(CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var posts = await db.DeploymentPosts.Where(p => p.TenantId == tenantId).OrderBy(p => p.Name).AsNoTracking().ToListAsync(ct);
        var rows = posts.Select((p, i) => new PositionFileRow(
            i + 1,
            p.Name,
            p.WindowStart,
            p.WindowEnd,
            p.RequiredHeadcount,
            p.MinRequiredFemale,
            p.MinRequiredMale,
            p.IsGenderIrrelevant,
            p.RequiredQualificationCode,
            p.BufferPercent,
            null));
        return Content(PositionFileParser.FormatExport(rows), "text/csv; charset=utf-8");
    }

    [HttpPost("positions/import/dry-run")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public ActionResult<object> DryRunPositions([FromBody] CsvImportRequest req)
    {
        var rows = PositionFileParser.Parse(req.CsvText);
        return Ok(new
        {
            rowCount = rows.Count,
            valid = rows.Count(r => r.Error is null),
            errors = rows.Where(r => r.Error is not null).Select(r => new { r.LineNumber, r.Error })
        });
    }

    [HttpPost("positions/import")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<PositionImportResult>> ImportPositions([FromBody] CsvImportRequest req, CancellationToken ct)
    {
        var result = await import.ImportPositionsAsync(User.GetTenantId(), req.CsvText, req.ReplaceAllPositions, ct);
        if (result.Errors.Count > 0) return BadRequest(result);
        await audit.WriteAsync(
            User.GetTenantId(),
            User.GetUserId(),
            "Positions.Import",
            "DeploymentPost",
            null,
            new { result.Created, result.RowCount },
            ct);
        return result;
    }

    [HttpGet("templates")]
    [AllowAnonymous]
    public ActionResult<object> Templates() =>
        Ok(new
        {
            personnelHeader = PersonnelFileParser.Header,
            positionsHeader = PositionFileParser.Header
        });
}
