namespace ShiftEngine.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DefaultLocale { get; set; } = "de-DE";
    public string BundeslandCode { get; set; } = "DE-SN";
    public decimal ReferenceContractHours { get; set; } = 174m;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>Optional outbound SMTP (demo/staging; use secrets in production).</summary>
    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SmtpFromEmail { get; set; }

    /// <summary>AI provider key or placeholder reference (encrypt at rest in production).</summary>
    public string? AiApiKeySecret { get; set; }
}
