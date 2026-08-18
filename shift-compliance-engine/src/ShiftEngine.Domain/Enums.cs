namespace ShiftEngine.Domain;

public enum LegacyReferenceMode
{
    ShadowPlanning = 0,
    Coexistence = 1,
    CutoverArchive = 2
}

public enum RosterPatternKind
{
    SixOnTwoOff = 0,
    SixOnThreeOff = 1,
    /// <summary>6 on / 2 off, then 6 on / 3 off; scaled from contracted monthly hours (174h reference).</summary>
    AlternatingSixTwoSixThree = 2
}

public enum LeaveSource
{
    Request = 0,
    CarryoverLocked = 1
}

public enum ShiftSwapStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum LedgerEntryKind
{
    SickLeave = 0,
    CallOut = 1,
    ActualHours = 2
}
