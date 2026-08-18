using ShiftEngine.Application.Compliance;
using ShiftEngine.Domain.Entities;

namespace ShiftEngine.Domain.Tests;

public class ComplianceRemediatorTests
{
    private static ShiftAssignment A(
        Guid empId,
        DateOnly day,
        TimeOnly start,
        TimeOnly end,
        Guid? postId = null,
        DeploymentPost? post = null) => new()
    {
        Id = Guid.NewGuid(),
        EmployeeId = empId,
        WorkDate = day,
        ShiftTier = new ShiftTier { StartLocal = start, EndLocal = end, Code = "X" },
        DeploymentPostId = postId,
        DeploymentPost = post,
    };

    private static Employee Emp(Guid id, string name, string role = "Security") => new()
    {
        Id = id,
        DisplayName = name,
        PrimaryRole = role,
        IsActive = true,
    };

    [Fact]
    public void Propose_DailyRest_RemovesSecondShiftWhenNoReplacement()
    {
        var emp = Guid.NewGuid();
        var assigns = new List<ShiftAssignment>
        {
            A(emp, new DateOnly(2026, 4, 1), new TimeOnly(22, 0), new TimeOnly(6, 0)),
            A(emp, new DateOnly(2026, 4, 2), new TimeOnly(6, 0), new TimeOnly(14, 0)),
        };
        var proposal = ComplianceRemediator.Propose(assigns, [], [Emp(emp, "A")]);
        Assert.NotEmpty(proposal.Actions);
        Assert.All(proposal.Actions, a => Assert.Equal(ComplianceRemediator.RemoveAssignment, a.Action));
        Assert.Empty(proposal.RemainingFindings.Where(f => f.IsBlocking));
    }

    [Fact]
    public void Propose_DailyRest_ReassignsWhenReplacementAvailable()
    {
        var emp = Guid.NewGuid();
        var replacement = Guid.NewGuid();
        var assigns = new List<ShiftAssignment>
        {
            A(emp, new DateOnly(2026, 4, 1), new TimeOnly(22, 0), new TimeOnly(6, 0)),
            A(emp, new DateOnly(2026, 4, 2), new TimeOnly(6, 0), new TimeOnly(14, 0)),
        };
        var employees = new List<Employee> { Emp(emp, "A"), Emp(replacement, "B") };
        var proposal = ComplianceRemediator.Propose(assigns, [], employees);
        Assert.Contains(proposal.Actions, a => a.Action == ComplianceRemediator.ReassignAssignment);
        Assert.Empty(proposal.RemainingFindings.Where(f => f.IsBlocking));
    }

    [Fact]
    public void Propose_WeeklyHours_PrefersReassignOverRemove()
    {
        var emp = Guid.NewGuid();
        var helper = Guid.NewGuid();
        var assigns = new List<ShiftAssignment>();
        var monday = new DateOnly(2026, 4, 6);
        for (var d = 0; d < 7; d++)
            assigns.Add(A(emp, monday.AddDays(d), new TimeOnly(6, 0), new TimeOnly(14, 0)));
        var employees = new List<Employee> { Emp(emp, "Over"), Emp(helper, "Free") };
        var proposal = ComplianceRemediator.Propose(assigns, [], employees);
        Assert.NotEmpty(proposal.Actions);
        Assert.Empty(proposal.RemainingFindings.Where(f => f.IsBlocking));
    }
}
