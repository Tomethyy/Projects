using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class SickReplanProposal : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid LedgerEntryId { get; set; }
    public DailyLedgerEntry LedgerEntry { get; set; } = null!;
    public string JsonPayload { get; set; } = "{}";
    public bool IsApplied { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
