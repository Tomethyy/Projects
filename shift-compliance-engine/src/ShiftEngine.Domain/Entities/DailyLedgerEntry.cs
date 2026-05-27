using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class DailyLedgerEntry : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateOnly EntryDate { get; set; }
    public LedgerEntryKind Kind { get; set; }
    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public Guid? ShiftAssignmentId { get; set; }
    public ShiftAssignment? ShiftAssignment { get; set; }
    public string? Notes { get; set; }
    public decimal? ActualHours { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
