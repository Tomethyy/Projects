using ShiftEngine.Application.Rostering;
using ShiftEngine.Domain.Entities;

namespace ShiftEngine.Domain.Tests;

public class PostAssignmentPlannerTests
{
    private static readonly Guid TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid PeriodId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid TierEarlyId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid TierLateId = Guid.Parse("00000000-0000-0000-0000-000000000011");

    [Fact]
    public void ResolveTierCode_FromPostName()
    {
        var post = new DeploymentPost { Name = "Früh GAT", WindowStart = new TimeOnly(6, 0), WindowEnd = new TimeOnly(14, 0) };
        Assert.Equal("EARLY", PostAssignmentPlanner.ResolveTierCode(post));
        post.Name = "Spät Tor 14";
        Assert.Equal("LATE", PostAssignmentPlanner.ResolveTierCode(post));
        post.Name = "Nacht 9";
        Assert.Equal("NIGHT", PostAssignmentPlanner.ResolveTierCode(post));
    }

    [Fact]
    public void Assign_GenderPost_PicksMaleAndFemale()
    {
        var postId = Guid.NewGuid();
        var post = new DeploymentPost
        {
            Id = postId,
            TenantId = TenantId,
            Name = "Früh GAT",
            WindowStart = new TimeOnly(6, 0),
            WindowEnd = new TimeOnly(14, 0),
            RequiredHeadcount = 2,
            MinRequiredFemale = 1,
            MinRequiredMale = 1,
            RequiredQualificationCode = "LSKP",
        };
        var f = new Employee
        {
            Id = Guid.NewGuid(),
            PersonnelNumber = "1001",
            PrimaryRole = "LSKP",
            GenderCode = "F",
        };
        var m = new Employee
        {
            Id = Guid.NewGuid(),
            PersonnelNumber = "1002",
            PrimaryRole = "LSKP",
            GenderCode = "M",
        };
        var x = new Employee
        {
            Id = Guid.NewGuid(),
            PersonnelNumber = "1003",
            PrimaryRole = "LSKP",
            GenderCode = "M",
        };
        var employees = new Dictionary<Guid, Employee> { [f.Id] = f, [m.Id] = m, [x.Id] = x };
        var day = new DateOnly(2026, 3, 10);
        var assignments = new List<ShiftAssignment>
        {
            NewAssignment(f.Id, day),
            NewAssignment(m.Id, day),
            NewAssignment(x.Id, day),
        };
        var tiers = new Dictionary<string, ShiftTier>
        {
            ["EARLY"] = new() { Id = TierEarlyId, Code = "EARLY" },
            ["LATE"] = new() { Id = TierLateId, Code = "LATE" },
            ["NIGHT"] = new() { Id = Guid.NewGuid(), Code = "NIGHT" },
        };

        var result = PostAssignmentPlanner.Assign(assignments, employees, [post], tiers);

        Assert.Equal(2, result.SlotsFilled);
        var onPost = assignments.Where(a => a.DeploymentPostId == postId).ToList();
        Assert.Equal(2, onPost.Count);
        Assert.Contains(onPost, a => PostAssignmentPlanner.IsFemale(employees[a.EmployeeId]));
        Assert.Contains(onPost, a => PostAssignmentPlanner.IsMale(employees[a.EmployeeId]));
        Assert.All(onPost, a => Assert.Equal(TierEarlyId, a.ShiftTierId));
    }

    [Fact]
    public void Assign_SecurityOnly_OnSecurityPost()
    {
        var postId = Guid.NewGuid();
        var post = new DeploymentPost
        {
            Id = postId,
            Name = "Früh 6",
            IsGenderIrrelevant = true,
            RequiredHeadcount = 1,
            RequiredQualificationCode = "Security",
        };
        var lskp = new Employee { Id = Guid.NewGuid(), PersonnelNumber = "2001", PrimaryRole = "LSKP", GenderCode = "M" };
        var sec = new Employee { Id = Guid.NewGuid(), PersonnelNumber = "2002", PrimaryRole = "Security", GenderCode = "M" };
        var employees = new Dictionary<Guid, Employee> { [lskp.Id] = lskp, [sec.Id] = sec };
        var day = new DateOnly(2026, 3, 11);
        var assignments = new List<ShiftAssignment> { NewAssignment(lskp.Id, day), NewAssignment(sec.Id, day) };
        var tiers = new Dictionary<string, ShiftTier> { ["EARLY"] = new() { Id = TierEarlyId, Code = "EARLY" } };

        PostAssignmentPlanner.Assign(assignments, employees, [post], tiers);

        Assert.Equal(sec.Id, assignments.Single(a => a.DeploymentPostId == postId).EmployeeId);
    }

    private static ShiftAssignment NewAssignment(Guid employeeId, DateOnly day) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        RosterPeriodId = PeriodId,
        EmployeeId = employeeId,
        ShiftTierId = TierEarlyId,
        WorkDate = day,
    };
}
