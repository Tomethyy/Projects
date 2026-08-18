using ShiftEngine.Domain;
using ShiftEngine.Domain.Rostering;

namespace ShiftEngine.Domain.Tests;

public class RosterRotationPlannerTests
{
    [Fact]
    public void BuildAlternatingCycle_At174h_IsSixTwoThenSixThree()
    {
        var c = RosterRotationPlanner.BuildAlternatingCycle(174m);
        Assert.Equal(17, c.Count);
        Assert.Equal(12, c.Count(x => x.IsWorkDay));
        Assert.True(c.Take(6).All(x => x.IsWorkDay));
        Assert.True(c.Skip(6).Take(2).All(x => !x.IsWorkDay));
        Assert.True(c.Skip(8).Take(6).All(x => x.IsWorkDay));
        Assert.True(c.Skip(14).Take(3).All(x => !x.IsWorkDay));
    }

    [Fact]
    public void BuildAlternatingCycle_PartTime_HasMoreRestThanFullTime()
    {
        var full = RosterRotationPlanner.BuildAlternatingCycle(174m);
        var part = RosterRotationPlanner.BuildAlternatingCycle(130m);
        Assert.True(part.Count > full.Count);
        Assert.True(part.Count(x => !x.IsWorkDay) > full.Count(x => !x.IsWorkDay));
        var fullYear = RosterRotationPlanner.EnumerateYear(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            RosterPatternKind.AlternatingSixTwoSixThree,
            174m,
            new DateOnly(2026, 1, 1)).Count(x => x.WorkDay);
        var partYear = RosterRotationPlanner.EnumerateYear(
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            RosterPatternKind.AlternatingSixTwoSixThree,
            130m,
            new DateOnly(2026, 1, 1)).Count(x => x.WorkDay);
        Assert.True(partYear < fullYear);
    }

    [Fact]
    public void ResolvePattern_NullOrAuto_UsesAlternating()
    {
        Assert.Equal(RosterPatternKind.AlternatingSixTwoSixThree, RosterRotationPlanner.ResolvePattern(174m, null));
        Assert.Equal(RosterPatternKind.AlternatingSixTwoSixThree, RosterRotationPlanner.ResolvePattern(130m, RosterPatternKind.AlternatingSixTwoSixThree));
    }

    [Fact]
    public void ResolvePattern_LegacyOverride_Respected()
    {
        Assert.Equal(RosterPatternKind.SixOnTwoOff, RosterRotationPlanner.ResolvePattern(174m, RosterPatternKind.SixOnTwoOff));
    }

    [Fact]
    public void EnumerateYear_Alternating_RespectsAnchor()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 17);
        var days = RosterRotationPlanner.EnumerateYear(
                start,
                end,
                RosterPatternKind.AlternatingSixTwoSixThree,
                174m,
                new DateOnly(2026, 1, 1))
            .ToList();
        Assert.Equal(17, days.Count);
        Assert.Equal(12, days.Count(x => x.WorkDay));
    }

    [Fact]
    public void BuildCycle_SixOnTwo_HasEightDays()
    {
        var c = RosterRotationPlanner.BuildCycle(RosterPatternKind.SixOnTwoOff, 174m);
        Assert.Equal(8, c.Count);
        Assert.Equal(6, c.Count(x => x.IsWorkDay));
    }
}
