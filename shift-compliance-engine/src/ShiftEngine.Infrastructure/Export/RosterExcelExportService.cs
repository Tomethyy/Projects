using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Infrastructure.Export;

public class RosterExcelExportService(AppDbContext db)
{
    public async Task<byte[]> ExportRosterAsync(Guid tenantId, Guid rosterPeriodId, CancellationToken ct = default)
    {
        var period = await db.RosterPeriods.AsNoTracking()
            .Include(p => p.Assignments).ThenInclude(a => a.Employee)
            .Include(p => p.Assignments).ThenInclude(a => a.ShiftTier)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == rosterPeriodId, ct)
            ?? throw new InvalidOperationException("Period not found");
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Roster");
        ws.Cell(1, 1).Value = "Date";
        ws.Cell(1, 2).Value = "Employee";
        ws.Cell(1, 3).Value = "Shift";
        var row = 2;
        foreach (var a in period.Assignments.OrderBy(x => x.WorkDate).ThenBy(x => x.Employee.DisplayName))
        {
            ws.Cell(row, 1).Value = a.WorkDate.ToString("o");
            ws.Cell(row, 2).Value = a.Employee.DisplayName;
            ws.Cell(row, 3).Value = a.ShiftTier.DisplayName;
            row++;
        }

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
