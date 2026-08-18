namespace ShiftEngine.Api.Models;

public sealed record RosterMatrixEmployeeRow(
    Guid Id,
    string PersonnelNumber,
    string DisplayName,
    decimal ContractedHoursMonthly);

public sealed record RosterMatrixCell(
    Guid EmployeeId,
    DateOnly Date,
    Guid? AssignmentId,
    Guid? ShiftTierId,
    string? TierCode,
    string? TierDisplayName,
    Guid? DeploymentPostId,
    string? PostName);

public sealed record RosterMatrixResponse(
    Guid PeriodId,
    int Year,
    int Month,
    string PeriodName,
    bool IsPublished,
    IReadOnlyList<DateOnly> Days,
    IReadOnlyList<RosterMatrixEmployeeRow> Employees,
    IReadOnlyList<RosterMatrixCell> Cells);

public sealed record RosterPeriodSummary(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsPublished);

public sealed record DeploymentGridSlot(string PersonnelNumber, string DisplayName, string TierCode);

public sealed record DeploymentGridCell(
    Guid PostId,
    string PostName,
    DateOnly Date,
    int RequiredHeadcount,
    IReadOnlyList<DeploymentGridSlot> Assigned);

public sealed record DeploymentGridResponse(
    Guid PeriodId,
    int Year,
    int Month,
    bool IsPublished,
    IReadOnlyList<DateOnly> Days,
    IReadOnlyList<DeploymentGridCell> Cells);
