using ShiftEngine.Domain;

namespace ShiftEngine.Domain.Tests;

public class SecurityRolesTests
{
    [Fact]
    public void WorksCouncilAuditor_is_reader_not_writer()
    {
        Assert.Contains(SecurityRoles.WorksCouncilAuditor, SecurityRoles.OperationsReaders);
        Assert.DoesNotContain(SecurityRoles.WorksCouncilAuditor, SecurityRoles.OperationsWriters);
    }

    [Fact]
    public void Admin_and_Planner_can_write()
    {
        foreach (var role in new[] { SecurityRoles.Admin, SecurityRoles.Planner })
        {
            Assert.Contains(role, SecurityRoles.OperationsWriters);
            Assert.Contains(role, SecurityRoles.OperationsReaders);
        }
    }
}
