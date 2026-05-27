using ShiftEngine.Domain.Common;

namespace ShiftEngine.Domain.Entities;

public class TrainingEvent : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string ParticipantEmployeeIdsCsv { get; set; } = string.Empty;
}
