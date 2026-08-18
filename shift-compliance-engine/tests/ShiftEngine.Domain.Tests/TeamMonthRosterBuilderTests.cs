using ShiftEngine.Application.Rostering;

namespace ShiftEngine.Domain.Tests;

public class TeamMonthRosterBuilderTests
{
    [Fact]
    public void EnumerateWorkDaysInMonth_FullTime_January_HasExpectedRange()
    {
        var days = TeamMonthRosterBuilder
            .EnumerateWorkDaysInMonth(2026, 1, 174m, new DateOnly(2026, 1, 1))
            .ToList();
        Assert.InRange(days.Count, 20, 24);
        Assert.All(days, d => Assert.Equal(1, d.Month));
    }

    [Fact]
    public void EnumerateWorkDaysInMonth_PartTime_FewerDaysThanFullTime()
    {
        var full = TeamMonthRosterBuilder
            .EnumerateWorkDaysInMonth(2026, 6, 174m, new DateOnly(2026, 6, 1))
            .Count();
        var part = TeamMonthRosterBuilder
            .EnumerateWorkDaysInMonth(2026, 6, 130m, new DateOnly(2026, 6, 1))
            .Count();
        Assert.True(part < full);
    }

    [Fact]
    public void FormatPeriodName_IsStable()
    {
        Assert.Equal("2026-03 Team", TeamMonthRosterBuilder.FormatPeriodName(2026, 3));
    }

    [Fact]
    public void StaggeredAnchor_SpreadsPhasesAcrossTeam()
    {
        const int cycleLen = 17;
        var baseAnchor = new DateOnly(2026, 3, 1);
        var a0 = TeamMonthRosterBuilder
            .EnumerateWorkDaysInMonth(2026, 3, 174m, TeamMonthRosterBuilder.StaggeredAnchor(baseAnchor, 0, 86, cycleLen))
            .ToHashSet();
        var a1 = TeamMonthRosterBuilder
            .EnumerateWorkDaysInMonth(2026, 3, 174m, TeamMonthRosterBuilder.StaggeredAnchor(baseAnchor, 1, 86, cycleLen))
            .ToHashSet();
        Assert.NotEqual(a0, a1);
        Assert.True(a0.Overlaps(a1), "Some overlap is expected; full overlap is not.");
        Assert.True(a0.Count > 0 && a1.Count > 0);
    }

    [Fact]
    public void StaggeredAnchor_SingleEmployee_KeepsBaseAnchor()
    {
        var baseAnchor = new DateOnly(2026, 3, 1);
        Assert.Equal(baseAnchor, TeamMonthRosterBuilder.StaggeredAnchor(baseAnchor, 0, 1, 17));
    }
}
