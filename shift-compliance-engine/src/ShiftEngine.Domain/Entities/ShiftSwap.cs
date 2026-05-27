using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class ShiftSwap : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RequesterEmployeeId { get; set; }
    public Employee RequesterEmployee { get; set; } = null!;
    public Guid TargetEmployeeId { get; set; }
    public Employee TargetEmployee { get; set; } = null!;
    public Guid RequesterAssignmentId { get; set; }
    public ShiftAssignment RequesterAssignment { get; set; } = null!;
    public Guid TargetAssignmentId { get; set; }
    public ShiftAssignment TargetAssignment { get; set; } = null!;
    public ShiftSwapStatus Status { get; set; } = ShiftSwapStatus.Pending;
    public string? ManagerUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
