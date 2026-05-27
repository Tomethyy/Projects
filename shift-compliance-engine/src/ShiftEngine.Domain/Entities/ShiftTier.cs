using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class ShiftTier : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public TimeOnly StartLocal { get; set; }
    public TimeOnly EndLocal { get; set; }
    public bool IsNight { get; set; }
    public bool IsSundayPremium { get; set; }
}
