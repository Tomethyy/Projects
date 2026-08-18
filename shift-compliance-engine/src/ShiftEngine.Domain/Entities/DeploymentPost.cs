using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class DeploymentPost : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public TimeOnly WindowStart { get; set; }
    public TimeOnly WindowEnd { get; set; }
    public int RequiredHeadcount { get; set; } = 1;
    /// <summary>Minimum staff identifying as female required when post is staffed (0 = no rule).</summary>
    public int MinRequiredFemale { get; set; }
    /// <summary>Minimum staff identifying as male required when post is staffed (0 = no rule).</summary>
    public int MinRequiredMale { get; set; }
    /// <summary>When true, gender mix rules are not applied for this post.</summary>
    public bool IsGenderIrrelevant { get; set; }
    public decimal BufferPercent { get; set; }
    public string? RequiredQualificationCode { get; set; }
    public string? AllowedRolesCsv { get; set; }
    public int StandardSecurityWeight { get; set; } = 100;
    public int LskpWeightOnSecurityPost { get; set; } = 60;
}
