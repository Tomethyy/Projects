namespace ShiftEngine.Domain;

public static class SecurityRoles
{
    public const string Admin = "Admin";
    public const string Planner = "Planner";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
    public const string WorksCouncilAuditor = "WorksCouncilAuditor";

    /// <summary>Mutating roster/ledger/leave operations.</summary>
    public const string OperationsWriters = $"{Admin},{Planner}";

    /// <summary>Read-only operational views (Betriebsrat, Führung).</summary>
    public const string OperationsReaders = $"{Admin},{Planner},{Manager},{WorksCouncilAuditor}";
}
