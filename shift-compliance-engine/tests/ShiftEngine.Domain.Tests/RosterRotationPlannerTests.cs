using ShiftEngine.Domain;
using ShiftEngine.Domain.Rostering;

namespace ShiftEngine.Domain.Tests;

public class RosterRotationPlannerTests
{
    [Fact]
    public void BuildCycle_SixOnTwo_HasEightDays()
    {
        var c = RosterRotationPlanner.BuildCycle(RosterPatternKind.SixOnTwoOff, 174m);
        Assert.Equal(8, c.Count);
        Assert.Equal(6, c.Count(x => x.IsWorkDay));
    }

    [Fact]
    public void EnumerateYear_RespectsPattern()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 1, 14);
        var days = RosterRotationPlanner.EnumerateYear(start, end, RosterPatternKind.SixOnTwoOff, 174m, new DateOnly(2026, 1, 1))
            .ToList();
        Assert.Equal(14, days.Count);
    }
}
