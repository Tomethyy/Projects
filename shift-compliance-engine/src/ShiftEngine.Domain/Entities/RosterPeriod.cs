using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class RosterPeriod : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsPublished { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? LegacySource { get; set; }
    public string? LegacyExternalId { get; set; }
    public LegacyReferenceMode? LegacyReferenceMode { get; set; }
    public ICollection<ShiftAssignment> Assignments { get; set; } = [];
}

public class ShiftAssignment : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid RosterPeriodId { get; set; }
    public RosterPeriod RosterPeriod { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid ShiftTierId { get; set; }
    public ShiftTier ShiftTier { get; set; } = null!;
    public DateOnly WorkDate { get; set; }
    public Guid? DeploymentPostId { get; set; }
    public DeploymentPost? DeploymentPost { get; set; }
}
