using ShiftEngine.Domain.Entities;

namespace ShiftEngine.Compliance.ArbZG;

public interface IComplianceRule
{
    string Code { get; }
    IEnumerable<ComplianceFinding> Evaluate(RosterEvaluationContext ctx);
}

public sealed record ComplianceFinding(string RuleCode, string Message, bool IsBlocking, Guid? EmployeeId, DateOnly? Date);

public sealed class RosterEvaluationContext
{
    public required Guid TenantId { get; init; }
    public required IReadOnlyList<ShiftAssignment> Assignments { get; init; }
    public required IReadOnlyList<LeaveRecord> Leaves { get; init; }
    public int PlanningYear { get; init; }
}

public sealed class WeeklyHoursRule : IComplianceRule
{
    public string Code => "ArbZG.WeeklyHours";

    public IEnumerable<ComplianceFinding> Evaluate(RosterEvaluationContext ctx)
    {
        const decimal maxWeekly = 48m;
        var byEmpWeek = ctx.Assignments
            .GroupBy(a => (a.EmployeeId, GetIsoWeek(a.WorkDate)))
            .Select(g => (g.Key, Hours: g.Sum(x => EstimateHours(x))));
        foreach (var ((empId, _), hours) in byEmpWeek)
        {
            if (hours > maxWeekly)
                yield return new ComplianceFinding(Code, $"Weekly hours {hours:0.#} exceed {maxWeekly} (employee {empId})", true, empId, null);
        }
    }

    private static int GetIsoWeek(DateOnly d) =>
        System.Globalization.ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue));

    private static decimal EstimateHours(ShiftAssignment a)
    {
        var start = a.ShiftTier.StartLocal;
        var end = a.ShiftTier.EndLocal;
        var day = a.WorkDate;
        var startDt = day.ToDateTime(start);
        var endDt = end > start ? day.ToDateTime(end) : day.ToDateTime(end).AddDays(1);
        return (decimal)(endDt - startDt).TotalHours;
    }
}

public sealed class DailyRestRule : IComplianceRule
{
    public string Code => "ArbZG.DailyRest";

    public IEnumerable<ComplianceFinding> Evaluate(RosterEvaluationContext ctx)
    {
        foreach (var g in ctx.Assignments.GroupBy(a => a.EmployeeId))
        {
            var days = g.OrderBy(x => x.WorkDate).ToList();
            for (var i = 1; i < days.Count; i++)
            {
                if (days[i].WorkDate == days[i - 1].WorkDate.AddDays(1))
                {
                    var prevEnd = days[i - 1].ShiftTier.EndLocal.ToTimeSpan();
                    var nextStart = days[i].ShiftTier.StartLocal.ToTimeSpan();
                    var rest = (TimeSpan.FromDays(1) - prevEnd) + nextStart;
                    if (rest.TotalHours < 11)
                        yield return new ComplianceFinding(Code, "Less than 11h rest between consecutive shifts", true, g.Key, days[i].WorkDate);
                }
            }
        }
    }
}

public static class ArbZGRuleRegistry
{
    public static IReadOnlyList<IComplianceRule> DefaultRules { get; } = [new WeeklyHoursRule(), new DailyRestRule()];
}
