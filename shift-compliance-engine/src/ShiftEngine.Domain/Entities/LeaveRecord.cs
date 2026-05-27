using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class LeaveRecord : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsApproved { get; set; }
    public LeaveSource Source { get; set; }
    public int CarryoverYear { get; set; }
    public bool LocksAvailability { get; set; } = true;

    /// <summary>When set, carryover balance lines must not be edited (year closed).</summary>
    public bool IsCarryoverFrozen { get; set; }
}
