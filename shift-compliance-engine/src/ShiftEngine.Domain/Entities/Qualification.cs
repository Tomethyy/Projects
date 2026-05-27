using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class Qualification : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int HierarchyWeight { get; set; }
}

public class EmployeeQualification : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public Guid QualificationId { get; set; }
    public Qualification Qualification { get; set; } = null!;
    public DateOnly? ValidUntil { get; set; }
}
