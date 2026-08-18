using ShiftEngine.Domain.Entities;

namespace ShiftEngine.Application.Rostering;

public sealed record PostAssignmentResult(
    int SlotsRequired,
    int SlotsFilled,
    int SlotsUnfilled,
    IReadOnlyList<string> Warnings);

/// <summary>Assigns deployment posts and shift tiers to existing work-day assignments.</summary>
public static class PostAssignmentPlanner
{
    public static PostAssignmentResult Assign(
        IList<ShiftAssignment> assignments,
        IReadOnlyDictionary<Guid, Employee> employeesById,
        IReadOnlyList<DeploymentPost> posts,
        IReadOnlyDictionary<string, ShiftTier> tiersByCode)
    {
        var warnings = new List<string>();
        var loadByEmployeePost = new Dictionary<(Guid EmployeeId, Guid PostId), int>();

        foreach (var dayGroup in assignments.GroupBy(a => a.WorkDate).OrderBy(g => g.Key))
        {
            var available = dayGroup.ToList();
            foreach (var post in posts.OrderByDescending(GenderConstraintScore).ThenBy(p => p.Name))
            {
                if (post.RequiredHeadcount <= 0) continue;
                if (!tiersByCode.TryGetValue(ResolveTierCode(post), out var tier))
                {
                    warnings.Add($"{dayGroup.Key}: Kein Schichtband für Post '{post.Name}'.");
                    continue;
                }

                var pool = available
                    .Where(a => RoleMatches(employeesById.GetValueOrDefault(a.EmployeeId), post))
                    .ToList();
                var picked = PickForPost(post, pool, loadByEmployeePost, employeesById);

                foreach (var a in picked)
                {
                    a.DeploymentPostId = post.Id;
                    a.ShiftTierId = tier.Id;
                    available.Remove(a);
                    var key = (a.EmployeeId, post.Id);
                    loadByEmployeePost[key] = loadByEmployeePost.GetValueOrDefault(key) + 1;
                }

                var shortfall = post.RequiredHeadcount - picked.Count;
                if (shortfall > 0)
                    warnings.Add(
                        $"{dayGroup.Key} · {post.Name}: {shortfall} von {post.RequiredHeadcount} nicht besetzt (Rolle/Geschlecht/Verfügbarkeit).");
            }

            if (available.Count > 0)
                warnings.Add($"{dayGroup.Key}: {available.Count} Arbeitstag(e) ohne Postenzuweisung.");
        }

        var workDays = assignments.Select(a => a.WorkDate).Distinct().Count();
        var slotsPerDay = posts.Sum(p => Math.Max(0, p.RequiredHeadcount));
        var slotsRequired = slotsPerDay * workDays;
        var slotsFilled = assignments.Count(a => a.DeploymentPostId is not null);
        return new PostAssignmentResult(slotsRequired, slotsFilled, Math.Max(0, slotsRequired - slotsFilled), warnings);
    }

    private static List<ShiftAssignment> PickForPost(
        DeploymentPost post,
        List<ShiftAssignment> pool,
        Dictionary<(Guid EmployeeId, Guid PostId), int> load,
        IReadOnlyDictionary<Guid, Employee> employeesById)
    {
        var need = post.RequiredHeadcount;
        if (need <= 0 || pool.Count == 0) return [];

        if (post.IsGenderIrrelevant)
            return Take(pool, need, post.Id, load, employeesById, _ => true);

        var picked = new List<ShiftAssignment>();
        var remaining = new List<ShiftAssignment>(pool);

        void take(Func<Employee?, bool> pred, int count)
        {
            foreach (var a in Take(remaining, count, post.Id, load, employeesById, pred))
            {
                picked.Add(a);
                remaining.Remove(a);
            }
        }

        take(IsFemale, post.MinRequiredFemale);
        take(IsMale, post.MinRequiredMale);

        foreach (var a in Take(remaining, need - picked.Count, post.Id, load, employeesById, _ => true))
            picked.Add(a);

        return picked;
    }

    private static List<ShiftAssignment> Take(
        List<ShiftAssignment> pool,
        int count,
        Guid postId,
        Dictionary<(Guid EmployeeId, Guid PostId), int> load,
        IReadOnlyDictionary<Guid, Employee> employeesById,
        Func<Employee?, bool> predicate) =>
        [.. pool
            .Where(a => predicate(employeesById.GetValueOrDefault(a.EmployeeId)))
            .OrderBy(a => load.GetValueOrDefault((a.EmployeeId, postId)))
            .ThenBy(a => employeesById.GetValueOrDefault(a.EmployeeId)?.PersonnelNumber ?? "")
            .Take(count)];

    private static int GenderConstraintScore(DeploymentPost p) =>
        p.IsGenderIrrelevant ? 0 : p.MinRequiredFemale + p.MinRequiredMale + p.RequiredHeadcount;

    public static string ResolveTierCode(DeploymentPost post)
    {
        var n = post.Name.Trim();
        if (n.StartsWith("Früh", StringComparison.OrdinalIgnoreCase) ||
            n.StartsWith("Frueh", StringComparison.OrdinalIgnoreCase))
            return "EARLY";
        if (n.StartsWith("Spät", StringComparison.OrdinalIgnoreCase) ||
            n.StartsWith("Spaet", StringComparison.OrdinalIgnoreCase))
            return "LATE";
        if (n.StartsWith("Nacht", StringComparison.OrdinalIgnoreCase))
            return "NIGHT";

        if (post.WindowStart.Hour >= 20 || post.WindowStart.Hour < 6)
            return "NIGHT";
        if (post.WindowStart.Hour >= 13)
            return "LATE";
        return "EARLY";
    }

    public static bool RoleMatches(Employee? employee, DeploymentPost post)
    {
        if (employee is null) return false;
        var req = post.RequiredQualificationCode?.Trim();
        if (string.IsNullOrEmpty(req)) return true;
        return string.Equals(employee.PrimaryRole, req, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsFemale(Employee? e) =>
        e?.GenderCode is not null &&
        (e.GenderCode.Equals("F", StringComparison.OrdinalIgnoreCase) ||
         e.GenderCode.Equals("W", StringComparison.OrdinalIgnoreCase));

    public static bool IsMale(Employee? e) =>
        e?.GenderCode is not null &&
        e.GenderCode.Equals("M", StringComparison.OrdinalIgnoreCase);
}
