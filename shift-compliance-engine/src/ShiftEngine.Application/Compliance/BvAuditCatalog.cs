namespace ShiftEngine.Application.Compliance;

/// <summary>
/// Betriebsvereinbarung audit checklist (template); extend with tenant-specific BV rules.
/// </summary>
public static class BvAuditCatalog
{
    public static IReadOnlyList<BvAuditFinding> DefaultChecklist(int planningYear) =>
    [
        new(
            "BV-RUHE-1",
            $"Planjahr {planningYear}: Ruhezeiten zwischen Schichtwechseln und Revierwechseln gegen BV-Text prüfen (kein Ersatz für Rechtsberatung).",
            "Info"),
        new(
            "BV-SONNTAG-1",
            "Sonntagsarbeit / Feiertage Sachsen: Einsatzvolumen und Ersatzruhetage dokumentieren.",
            "Info"),
        new(
            "BV-MITBESTIMMUNG-1",
            "Änderungen an Schichtmodellen und Pausenregeln: Mitbestimmungsrechte (§87 BetrVG) abstimmen.",
            "Reminder")
    ];
}

public sealed record BvAuditFinding(string Code, string MessageDe, string Severity);
