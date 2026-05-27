namespace ShiftEngine.Application.WhatIf;

public sealed record RosterScenarioDraft(Guid Id, string Name, Guid? BaseRosterPeriodId);

/// <summary>Placeholder for Tier 4 what-if sandbox.</summary>
public static class RosterScenarioService
{
    public static RosterScenarioDraft CreateDraft(string name, Guid? baseRosterPeriodId) =>
        new(Guid.NewGuid(), name, baseRosterPeriodId);
}
