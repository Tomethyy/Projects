namespace ShiftEngine.Application.Imports;

public static class SemicolonCsv
{
    public static IReadOnlyList<string[]> ParseRows(string text, char separator = ';')
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        return [.. text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split(separator).Select(c => c.Trim()).ToArray())];
    }

    public static bool RowIsHeader(string[] cells, params string[] expected) =>
        cells.Length > 0 && expected.Any(e =>
            cells[0].Contains(e, StringComparison.OrdinalIgnoreCase));

    public static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }

    public static string JoinRow(params string?[] cells) =>
        string.Join(';', cells.Select(Escape));
}
