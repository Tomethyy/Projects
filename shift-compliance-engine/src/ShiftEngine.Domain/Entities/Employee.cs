using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class Employee : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? LinkedUserId { get; set; }
    public string PersonnelNumber { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? ExternalLegacyId { get; set; }
    public decimal ContractedHoursMonthly { get; set; } = 174m;
    public string PrimaryRole { get; set; } = "Security";
    public string? GenderCode { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<EmployeeQualification> Qualifications { get; set; } = [];
}
