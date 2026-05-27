using ClosedXML.Excel;

namespace ShiftEngine.Application.Imports;

public sealed record SecPlanImportRow(int RowNumber, string? PersonnelNumber, string? DisplayName, string? ExternalLegacyId, string? Error);

public sealed class SecPlanImportResult
{
    public List<SecPlanImportRow> Rows { get; } = [];
    public bool HasErrors => Rows.Any(r => r.Error != null);
}

/// <summary>Optional SecPlan-mapped template: columns PersonnelNumber, DisplayName, ExternalLegacyId (row 1 = headers).</summary>
public static class SecPlanExcelImporter
{
    public static SecPlanImportResult DryRun(Stream xlsx)
    {
        var result = new SecPlanImportResult();
        using var wb = new XLWorkbook(xlsx);
        var ws = wb.Worksheets.First();
        var first = ws.FirstRowUsed()?.RowNumber() ?? 1;
        var start = first == 1 && ws.Cell(1, 1).GetString().Contains("Personnel", StringComparison.OrdinalIgnoreCase) ? 2 : 1;
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= start))
        {
            var pn = row.Cell(1).GetString().Trim();
            var dn = row.Cell(2).GetString().Trim();
            var ext = row.Cell(3).GetString().Trim();
            string? err = null;
            if (string.IsNullOrWhiteSpace(pn)) err = "PersonnelNumber required";
            result.Rows.Add(new SecPlanImportRow(row.RowNumber(), pn, dn, ext, err));
        }

        return result;
    }
}
