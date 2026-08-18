namespace ShiftEngine.Application.Imports;

public sealed record PersonnelFileRow(
    int LineNumber,
    string PersonnelNumber,
    string DisplayName,
    decimal ContractedHoursMonthly,
    string? GenderCode,
    string PrimaryRole,
    string? Email,
    string? ExternalLegacyId,
    string? Error);

public sealed record PositionFileRow(
    int LineNumber,
    string Name,
    TimeOnly WindowStart,
    TimeOnly WindowEnd,
    int RequiredHeadcount,
    int MinRequiredFemale,
    int MinRequiredMale,
    bool IsGenderIrrelevant,
    string? RequiredQualificationCode,
    decimal BufferPercent,
    string? Error);

public static class PersonnelFileParser
{
    public const string Header =
        "PersonnelNumber;DisplayName;ContractedHoursMonthly;GenderCode;PrimaryRole;Email;ExternalLegacyId";

    public static IReadOnlyList<PersonnelFileRow> Parse(string csv)
    {
        var rows = SemicolonCsv.ParseRows(csv);
        if (rows.Count == 0) return [];

        var start = SemicolonCsv.RowIsHeader(rows[0], "PersonnelNumber") ? 1 : 0;
        var result = new List<PersonnelFileRow>();
        for (var i = start; i < rows.Count; i++)
        {
            var cells = rows[i];
            var line = i + 1;
            var pn = Cell(cells, 0);
            var dn = Cell(cells, 1);
            var hoursText = Cell(cells, 2);
            var gender = NullIfEmpty(Cell(cells, 3));
            var role = NullIfEmpty(Cell(cells, 4)) ?? "Security";
            var email = NullIfEmpty(Cell(cells, 5));
            var ext = NullIfEmpty(Cell(cells, 6));

            if (string.IsNullOrWhiteSpace(pn))
            {
                result.Add(new PersonnelFileRow(line, pn, dn, 0, gender, role, email, ext, "PersonnelNumber required"));
                continue;
            }

            if (string.IsNullOrWhiteSpace(dn))
            {
                result.Add(new PersonnelFileRow(line, pn, dn, 0, gender, role, email, ext, "DisplayName required"));
                continue;
            }

            if (!decimal.TryParse(hoursText, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var hours) || hours <= 0)
            {
                result.Add(new PersonnelFileRow(line, pn, dn, 0, gender, role, email, ext,
                    "ContractedHoursMonthly must be a positive number"));
                continue;
            }

            result.Add(new PersonnelFileRow(line, pn, dn, hours, gender, role, email, ext, null));
        }

        return result;
    }

    public static string FormatExport(IEnumerable<PersonnelFileRow> rows) =>
        Header + "\n" + string.Join('\n', rows.Where(r => r.Error is null).Select(r =>
            SemicolonCsv.JoinRow(
                r.PersonnelNumber,
                r.DisplayName,
                r.ContractedHoursMonthly.ToString(System.Globalization.CultureInfo.InvariantCulture),
                r.GenderCode,
                r.PrimaryRole,
                r.Email,
                r.ExternalLegacyId)));

    private static string Cell(string[] cells, int index) =>
        index < cells.Length ? cells[index].Trim() : string.Empty;

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

public static class PositionFileParser
{
    public const string Header =
        "Name;WindowStart;WindowEnd;RequiredHeadcount;MinRequiredFemale;MinRequiredMale;GenderIrrelevant;RequiredQualificationCode;BufferPercent";

    public static IReadOnlyList<PositionFileRow> Parse(string csv)
    {
        var rows = SemicolonCsv.ParseRows(csv);
        if (rows.Count == 0) return [];

        var start = SemicolonCsv.RowIsHeader(rows[0], "Name", "PostName") ? 1 : 0;
        var result = new List<PositionFileRow>();
        for (var i = start; i < rows.Count; i++)
        {
            var cells = rows[i];
            var line = i + 1;
            var name = Cell(cells, 0);
            var startText = Cell(cells, 1);
            var endText = Cell(cells, 2);
            var headText = Cell(cells, 3);
            var minFText = Cell(cells, 4);
            var minMText = Cell(cells, 5);
            var hasGenderColumn = cells.Length >= 9;
            var genderText = hasGenderColumn ? Cell(cells, 6) : string.Empty;
            var qual = NullIfEmpty(hasGenderColumn ? Cell(cells, 7) : Cell(cells, 6));
            var bufferText = hasGenderColumn ? Cell(cells, 8) : Cell(cells, 7);

            if (string.IsNullOrWhiteSpace(name))
            {
                result.Add(new PositionFileRow(line, name, default, default, 0, 0, 0, true, qual, 0, "Name required"));
                continue;
            }

            if (!TimeOnly.TryParse(startText, out var ws))
            {
                result.Add(new PositionFileRow(line, name, default, default, 0, 0, 0, true, qual, 0,
                    "WindowStart invalid (use HH:mm)"));
                continue;
            }

            if (!TimeOnly.TryParse(endText, out var we))
            {
                result.Add(new PositionFileRow(line, name, ws, default, 0, 0, 0, true, qual, 0,
                    "WindowEnd invalid (use HH:mm)"));
                continue;
            }

            if (!int.TryParse(headText, out var head) || head < 1)
            {
                result.Add(new PositionFileRow(line, name, ws, we, 0, 0, 0, true, qual, 0,
                    "RequiredHeadcount must be >= 1"));
                continue;
            }

            if (!int.TryParse(string.IsNullOrWhiteSpace(minFText) ? "0" : minFText, out var minF) || minF < 0)
            {
                result.Add(new PositionFileRow(line, name, ws, we, head, 0, 0, true, qual, 0,
                    "MinRequiredFemale must be >= 0"));
                continue;
            }

            if (!int.TryParse(string.IsNullOrWhiteSpace(minMText) ? "0" : minMText, out var minM) || minM < 0)
            {
                result.Add(new PositionFileRow(line, name, ws, we, head, minF, 0, true, qual, 0,
                    "MinRequiredMale must be >= 0"));
                continue;
            }

            bool genderIrrelevant;
            if (hasGenderColumn)
            {
                if (!TryParseGenderIrrelevantCell(genderText, out genderIrrelevant, out var genderErr))
                {
                    result.Add(new PositionFileRow(line, name, ws, we, head, minF, minM, true, qual, 0, genderErr));
                    continue;
                }
            }
            else
                genderIrrelevant = minF == 0 && minM == 0;

            if (genderIrrelevant)
            {
                minF = 0;
                minM = 0;
            }

            if (!decimal.TryParse(string.IsNullOrWhiteSpace(bufferText) ? "0" : bufferText,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var buffer) || buffer < 0)
            {
                result.Add(new PositionFileRow(line, name, ws, we, head, minF, minM, genderIrrelevant, qual, 0,
                    "BufferPercent invalid"));
                continue;
            }

            result.Add(new PositionFileRow(line, name, ws, we, head, minF, minM, genderIrrelevant, qual, buffer, null));
        }

        return result;
    }

    public static string FormatExport(IEnumerable<PositionFileRow> rows) =>
        Header + "\n" + string.Join('\n', rows.Where(r => r.Error is null).Select(r =>
            SemicolonCsv.JoinRow(
                r.Name,
                r.WindowStart.ToString("HH:mm"),
                r.WindowEnd.ToString("HH:mm"),
                r.RequiredHeadcount.ToString(),
                r.MinRequiredFemale.ToString(),
                r.MinRequiredMale.ToString(),
                r.IsGenderIrrelevant ? "1" : "0",
                r.RequiredQualificationCode,
                r.BufferPercent.ToString(System.Globalization.CultureInfo.InvariantCulture))));

    private static bool TryParseGenderIrrelevantCell(string text, out bool isIrrelevant, out string? error)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            isIrrelevant = false;
            error = null;
            return true;
        }

        if (IsKnownGenderToken(text))
        {
            isIrrelevant = ParseGenderIrrelevantValue(text);
            error = null;
            return true;
        }

        isIrrelevant = false;
        error = "GenderIrrelevant use 0/1, yes/no, relevant/irrelevant (or unerheblich/relevant)";
        return false;
    }

    private static bool IsKnownGenderToken(string text)
    {
        var t = text.Trim();
        return t.Equals("0", StringComparison.OrdinalIgnoreCase)
            || t.Equals("1", StringComparison.OrdinalIgnoreCase)
            || t.Equals("true", StringComparison.OrdinalIgnoreCase)
            || t.Equals("false", StringComparison.OrdinalIgnoreCase)
            || t.Equals("yes", StringComparison.OrdinalIgnoreCase)
            || t.Equals("no", StringComparison.OrdinalIgnoreCase)
            || t.Equals("ja", StringComparison.OrdinalIgnoreCase)
            || t.Equals("nein", StringComparison.OrdinalIgnoreCase)
            || t.Equals("irrelevant", StringComparison.OrdinalIgnoreCase)
            || t.Equals("unerheblich", StringComparison.OrdinalIgnoreCase)
            || t.Equals("relevant", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ParseGenderIrrelevantValue(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        var t = text.Trim();
        return t is "1" or "true" or "yes" or "ja"
            || t.Equals("irrelevant", StringComparison.OrdinalIgnoreCase)
            || t.Equals("unerheblich", StringComparison.OrdinalIgnoreCase);
    }

    private static string Cell(string[] cells, int index) =>
        index < cells.Length ? cells[index].Trim() : string.Empty;

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
