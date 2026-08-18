using ShiftEngine.Domain;
using ShiftEngine.Domain.Rostering;

namespace ShiftEngine.Application.Rostering;

/// <summary>Team-wide monthly roster planning (Phase 1).</summary>
public static class TeamMonthRosterBuilder
{
    public static string FormatPeriodName(int year, int month) => $"{year:0000}-{month:00} Team";

    public static (DateOnly Start, DateOnly End) GetMonthBounds(int year, int month)
    {
        var start = new DateOnly(year, month, 1);
        return (start, start.AddMonths(1).AddDays(-1));
    }

    public static DateOnly DefaultAnchor(int year, int month) => new(year, month, 1);

    /// <summary>
    /// Spreads rotation phase across the team so not everyone works the same calendar days.
    /// Shifts the anchor backward by evenly spaced steps along each employee's cycle.
    /// </summary>
    public static DateOnly StaggeredAnchor(
        DateOnly baseAnchor,
        int employeeIndex,
        int employeeCount,
        int cycleLengthDays)
    {
        if (employeeCount <= 1 || cycleLengthDays <= 0 || employeeIndex <= 0)
            return baseAnchor;
        var step = cycleLengthDays / employeeCount;
        if (step <= 0)
            step = 1;
        var shift = employeeIndex * step % cycleLengthDays;
        return baseAnchor.AddDays(-shift);
    }

    public static IEnumerable<DateOnly> EnumerateWorkDaysInMonth(
        int year,
        int month,
        decimal contractedHoursMonthly,
        DateOnly anchorFirstWorkDay,
        RosterPatternKind? patternOverride = null)
    {
        var (start, end) = GetMonthBounds(year, month);
        var pattern = RosterRotationPlanner.ResolvePattern(contractedHoursMonthly, patternOverride);
        foreach (var (d, work) in RosterRotationPlanner.EnumerateYear(
                     start,
                     end,
                     pattern,
                     contractedHoursMonthly,
                     anchorFirstWorkDay))
        {
            if (work) yield return d;
        }
    }
}
