using ShiftEngine.Domain;

namespace ShiftEngine.Domain.Rostering;

public sealed record RotationDay(bool IsWorkDay);

/// <summary>
/// Builds work/off rhythms from contracted monthly hours.
/// Full-time reference (174h): repeating 6 work + 2 off, then 6 work + 3 off.
/// Part-time: extra rest days (and optionally shorter work blocks) so annual load tracks hours.
/// </summary>
public static class RosterRotationPlanner
{
    public const decimal ReferenceHours = 174m;

    /// <summary>Default 6/2 then 6/3 block lengths at reference hours (174h/month).</summary>
    public const int ReferenceWorkStreak = 6;
    public const int ReferenceRestAfterFirstBlock = 2;
    public const int ReferenceRestAfterSecondBlock = 3;

    public static int ReferenceCycleLength =>
        ReferenceWorkStreak + ReferenceRestAfterFirstBlock + ReferenceWorkStreak + ReferenceRestAfterSecondBlock;

    public static RosterPatternKind ResolvePattern(decimal contractedMonthlyHours, RosterPatternKind? requested) =>
        requested is RosterPatternKind.SixOnTwoOff or RosterPatternKind.SixOnThreeOff
            ? requested.Value
            : RosterPatternKind.AlternatingSixTwoSixThree;

    public static IReadOnlyList<RotationDay> BuildCycle(
        RosterPatternKind pattern,
        decimal contractedMonthlyHours)
    {
        if (pattern == RosterPatternKind.AlternatingSixTwoSixThree)
            return BuildAlternatingCycle(contractedMonthlyHours);

        var scale = contractedMonthlyHours <= 0 ? 1m : Math.Min(1.2m, contractedMonthlyHours / ReferenceHours);
        var (work, rest) = pattern switch
        {
            RosterPatternKind.SixOnTwoOff => (ReferenceWorkStreak, ReferenceRestAfterFirstBlock),
            RosterPatternKind.SixOnThreeOff => (ReferenceWorkStreak, ReferenceRestAfterSecondBlock),
            _ => (ReferenceWorkStreak, ReferenceRestAfterFirstBlock)
        };
        if (scale < 1m)
            rest = Math.Max(rest, (int)Math.Ceiling(rest / (double)scale));

        return [.. WorkBlock(work), .. RestBlock(rest)];
    }

    /// <summary>
    /// 6/2 then 6/3 at 174h; scales total rest (2:3 split) when monthly hours are lower.
    /// </summary>
    public static IReadOnlyList<RotationDay> BuildAlternatingCycle(decimal contractedMonthlyHours)
    {
        var ratio = contractedMonthlyHours <= 0 ? 1m : Math.Min(1.2m, contractedMonthlyHours / ReferenceHours);
        var work = ratio switch
        {
            < 0.45m => 4,
            < 0.65m => 5,
            _ => ReferenceWorkStreak
        };

        var restA = ReferenceRestAfterFirstBlock;
        var restB = ReferenceRestAfterSecondBlock;
        if (ratio < 1m)
        {
            var offBase = restA + restB;
            var offScaled = Math.Max(offBase, (int)Math.Ceiling(offBase / (double)ratio));
            restA = Math.Max(ReferenceRestAfterFirstBlock, (int)Math.Round(offScaled * 2.0 / 5.0, MidpointRounding.AwayFromZero));
            restB = Math.Max(ReferenceRestAfterSecondBlock, offScaled - restA);
        }

        return [.. WorkBlock(work), .. RestBlock(restA), .. WorkBlock(work), .. RestBlock(restB)];
    }

    public static string DescribeCycle(IReadOnlyList<RotationDay> cycle)
    {
        var work = cycle.Count(d => d.IsWorkDay);
        return $"{work}/{cycle.Count} work days per cycle ({cycle.Count}d rhythm)";
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

    public static decimal EstimateHoursPerShift(TimeOnly start, TimeOnly end)
    {
        var span = end > start ? end - start : end.Add(TimeSpan.FromDays(1)) - start;
        return (decimal)span.TotalHours;
    }

    public static decimal EstimateAnnualHours(
        DateOnly yearStart,
        DateOnly yearEnd,
        RosterPatternKind pattern,
        decimal contractedMonthlyHours,
        DateOnly anchorFirstWorkDay,
        decimal hoursPerWorkDay)
    {
        var workDays = EnumerateYear(yearStart, yearEnd, pattern, contractedMonthlyHours, anchorFirstWorkDay)
            .Count(x => x.WorkDay);
        return workDays * hoursPerWorkDay;
    }

    private static IEnumerable<RotationDay> WorkBlock(int days) =>
        Enumerable.Range(0, days).Select(_ => new RotationDay(true));

    private static IEnumerable<RotationDay> RestBlock(int days) =>
        Enumerable.Range(0, days).Select(_ => new RotationDay(false));
}
