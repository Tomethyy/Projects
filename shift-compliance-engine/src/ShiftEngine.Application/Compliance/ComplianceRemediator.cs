using ShiftEngine.Compliance.ArbZG;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Replanning;

namespace ShiftEngine.Application.Compliance;

public sealed record ComplianceFixAction(
    string RuleCode,
    Guid AssignmentId,
    Guid EmployeeId,
    DateOnly WorkDate,
    string Action,
    string Reason,
    Guid? TargetEmployeeId = null,
    string? TargetEmployeeName = null);

public sealed record ComplianceFixProposal(
    IReadOnlyList<ComplianceFixAction> Actions,
    IReadOnlyList<ComplianceFinding> RemainingFindings,
    int BlockingBefore,
    int BlockingAfter);

public static class ComplianceRemediator
{
    public const string RemoveAssignment = "RemoveAssignment";
    public const string ReassignAssignment = "ReassignAssignment";

    public static ComplianceFixProposal Propose(
        IReadOnlyList<ShiftAssignment> assignments,
        IReadOnlyList<LeaveRecord> leaves,
        IReadOnlyList<Employee> employees)
    {
        var working = assignments.Select(CloneAssignment).ToList();
        var actions = new List<ComplianceFixAction>();

        for (var step = 0; step < 500; step++)
        {
            var findings = Evaluate(working, leaves);
            var blocking = findings.FirstOrDefault(f => f.IsBlocking);
            if (blocking is null) break;
            var fix = DeriveFix(blocking, working, employees);
            if (fix is null) break;
            actions.Add(fix);
            ApplyFixToWorking(fix, working);
        }

        var remaining = Evaluate(working, leaves);
        var blockingBefore = Evaluate(assignments, leaves).Count(f => f.IsBlocking);
        return new ComplianceFixProposal(actions, remaining, blockingBefore, remaining.Count(f => f.IsBlocking));
    }

    private static ShiftAssignment CloneAssignment(ShiftAssignment a) => new()
    {
        Id = a.Id,
        TenantId = a.TenantId,
        RosterPeriodId = a.RosterPeriodId,
        EmployeeId = a.EmployeeId,
        Employee = a.Employee,
        ShiftTierId = a.ShiftTierId,
        ShiftTier = a.ShiftTier,
        WorkDate = a.WorkDate,
        DeploymentPostId = a.DeploymentPostId,
        DeploymentPost = a.DeploymentPost,
    };

    private static void ApplyFixToWorking(ComplianceFixAction fix, List<ShiftAssignment> working)
    {
        var assignment = working.FirstOrDefault(a => a.Id == fix.AssignmentId);
        if (assignment is null) return;
        if (fix.Action == ReassignAssignment && fix.TargetEmployeeId.HasValue)
            assignment.EmployeeId = fix.TargetEmployeeId.Value;
        else
            working.Remove(assignment);
    }

    private static List<ComplianceFinding> Evaluate(IReadOnlyList<ShiftAssignment> working, IReadOnlyList<LeaveRecord> leaves)
    {
        var year = working.Count > 0 ? working.Max(a => a.WorkDate.Year) : DateTime.UtcNow.Year;
        var ctx = new RosterEvaluationContext
        {
            TenantId = Guid.Empty,
            Assignments = working,
            Leaves = leaves,
            PlanningYear = year,
        };
        return [.. ArbZGRuleRegistry.DefaultRules.SelectMany(r => r.Evaluate(ctx))];
    }

    private static ComplianceFixAction? DeriveFix(
        ComplianceFinding finding,
        List<ShiftAssignment> working,
        IReadOnlyList<Employee> employees) =>
        finding.RuleCode switch
        {
            "ArbZG.DailyRest" => FixDailyRest(finding, working, employees),
            "ArbZG.WeeklyHours" => FixWeeklyHours(finding, working, employees),
            _ => null,
        };

    private static ComplianceFixAction? FixDailyRest(
        ComplianceFinding finding,
        List<ShiftAssignment> working,
        IReadOnlyList<Employee> employees)
    {
        if (finding.EmployeeId is null || finding.Date is null) return null;
        var target = working.FirstOrDefault(a => a.EmployeeId == finding.EmployeeId && a.WorkDate == finding.Date);
        if (target is null) return null;

        var reassign = TryReassign(target, working, employees, finding.RuleCode,
            "Schicht an qualifizierten Ersatzmitarbeitenden übergeben für 11 h Ruhezeit.");
        if (reassign is not null) return reassign;

        return new ComplianceFixAction(
            finding.RuleCode, target.Id, target.EmployeeId, target.WorkDate, RemoveAssignment,
            "Schicht entfernen für mindestens 11 h Ruhezeit zwischen aufeinanderfolgenden Schichten.");
    }

    private static ComplianceFixAction? FixWeeklyHours(
        ComplianceFinding finding,
        List<ShiftAssignment> working,
        IReadOnlyList<Employee> employees)
    {
        if (finding.EmployeeId is null) return null;
        const decimal maxWeekly = 48m;
        var empId = finding.EmployeeId.Value;
        var violatingWeeks = working
            .Where(a => a.EmployeeId == empId)
            .GroupBy(a => GetIsoWeek(a.WorkDate))
            .Where(g => g.Sum(EstimateHours) > maxWeekly)
            .OrderByDescending(g => g.Sum(EstimateHours));

        foreach (var week in violatingWeeks)
        {
            var candidate = week
                .OrderBy(a => a.DeploymentPostId.HasValue ? 1 : 0)
                .ThenByDescending(EstimateHours)
                .ThenBy(a => a.WorkDate)
                .FirstOrDefault();
            if (candidate is null) continue;

            var reassign = TryReassign(candidate, working, employees, finding.RuleCode,
                $"Schicht übergeben, damit Wochenarbeitszeit ≤ {maxWeekly:0} h bleibt.");
            if (reassign is not null) return reassign;

            return new ComplianceFixAction(
                finding.RuleCode, candidate.Id, candidate.EmployeeId, candidate.WorkDate, RemoveAssignment,
                $"Schicht entfernen, damit Wochenarbeitszeit ≤ {maxWeekly:0} h ist.");
        }

        return null;
    }

    private static ComplianceFixAction? TryReassign(
        ShiftAssignment target,
        List<ShiftAssignment> working,
        IReadOnlyList<Employee> employees,
        string ruleCode,
        string reason)
    {
        var sameDay = working.Where(a => a.WorkDate == target.WorkDate).ToList();
        var ranked = SickLeaveCandidateRanker.Rank(target, employees, sameDay, target.DeploymentPost);
        foreach (var c in ranked)
        {
            if (!CanTakeAssignment(c.EmployeeId, target, working)) continue;
            return new ComplianceFixAction(
                ruleCode, target.Id, target.EmployeeId, target.WorkDate, ReassignAssignment, reason,
                c.EmployeeId, c.DisplayName);
        }

        return null;
    }

    private static bool CanTakeAssignment(Guid employeeId, ShiftAssignment slot, IReadOnlyList<ShiftAssignment> working)
    {
        if (working.Any(a => a.EmployeeId == employeeId && a.WorkDate == slot.WorkDate && a.Id != slot.Id))
            return false;

        var hypothetical = working
            .Where(a => a.Id != slot.Id)
            .Append(new ShiftAssignment
            {
                Id = slot.Id,
                EmployeeId = employeeId,
                WorkDate = slot.WorkDate,
                ShiftTier = slot.ShiftTier,
                DeploymentPostId = slot.DeploymentPostId,
            })
            .Where(a => a.EmployeeId == employeeId)
            .OrderBy(a => a.WorkDate)
            .ToList();

        for (var i = 1; i < hypothetical.Count; i++)
        {
            if (hypothetical[i].WorkDate != hypothetical[i - 1].WorkDate.AddDays(1)) continue;
            var prev = hypothetical[i - 1];
            var prevEnd = RestEnd(prev);
            var nextStart = hypothetical[i].WorkDate.ToDateTime(hypothetical[i].ShiftTier.StartLocal);
            if ((nextStart - prevEnd).TotalHours < 11) return false;
        }

        const decimal maxWeekly = 48m;
        var week = GetIsoWeek(slot.WorkDate);
        var weekHours = working
            .Where(a => a.EmployeeId == employeeId && GetIsoWeek(a.WorkDate) == week && a.Id != slot.Id)
            .Sum(EstimateHours) + EstimateHours(slot);
        return weekHours <= maxWeekly;
    }

    private static DateTime RestEnd(ShiftAssignment a)
    {
        var day = a.WorkDate;
        return a.ShiftTier.EndLocal > a.ShiftTier.StartLocal
            ? day.ToDateTime(a.ShiftTier.EndLocal)
            : day.ToDateTime(a.ShiftTier.EndLocal).AddDays(1);
    }

    private static int GetIsoWeek(DateOnly d) =>
        System.Globalization.ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue));

    private static decimal EstimateHours(ShiftAssignment a)
    {
        var start = a.ShiftTier.StartLocal;
        var end = a.ShiftTier.EndLocal;
        var day = a.WorkDate;
        var startDt = day.ToDateTime(start);
        var endDt = end > start ? day.ToDateTime(end) : day.ToDateTime(end).AddDays(1);
        return (decimal)(endDt - startDt).TotalHours;
    }
}
