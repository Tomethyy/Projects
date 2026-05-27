using ShiftEngine.Domain;

namespace ShiftEngine.Domain.Rostering;

public sealed record RotationDay(bool IsWorkDay);

/// <summary>
/// Builds repeating work/off patterns for 6/2 and 6/3 from a 174h reference; scales cycle when contracted hours differ.
/// </summary>
public static class RosterRotationPlanner
{
    public const decimal ReferenceHours = 174m;

    public static IReadOnlyList<RotationDay> BuildCycle(RosterPatternKind pattern, decimal contractedMonthlyHours)
    {
        var scale = contractedMonthlyHours <= 0 ? 1m : Math.Min(1.2m, contractedMonthlyHours / ReferenceHours);
        var (work, rest) = pattern switch
        {
            RosterPatternKind.SixOnTwoOff => (6, 2),
            RosterPatternKind.SixOnThreeOff => (6, 3),
            _ => (6, 2)
        };
        if (scale < 1m)
        {
            rest = Math.Max(rest, (int)Math.Ceiling(rest / (double)scale));
        }

        return [.. Enumerable.Range(0, work).Select(_ => new RotationDay(true)), .. Enumerable.Range(0, rest).Select(_ => new RotationDay(false))];
    }

    public static IEnumerable<(DateOnly Date, bool WorkDay)> EnumerateYear(
        DateOnly yearStart,
        DateOnly yearEnd,
        RosterPatternKind pattern,
        decimal contractedMonthlyHours,
        DateOnly anchorFirstWorkDay)
    {
        var cycle = BuildCycle(pattern, contractedMonthlyHours);
        for (var d = yearStart; d <= yearEnd; d = d.AddDays(1))
        {
            var offset = (d.DayNumber - anchorFirstWorkDay.DayNumber) % cycle.Count;
            if (offset < 0) offset += cycle.Count;
            yield return (d, cycle[offset].IsWorkDay);
        }
    }
}
