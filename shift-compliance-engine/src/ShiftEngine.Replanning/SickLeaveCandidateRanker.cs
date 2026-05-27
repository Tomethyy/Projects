using ShiftEngine.Domain.Entities;

namespace ShiftEngine.Replanning;

public sealed record ReplacementCandidate(Guid EmployeeId, string DisplayName, int Score, string Reason);

public static class SickLeaveCandidateRanker
{
    public static IReadOnlyList<ReplacementCandidate> Rank(
        ShiftAssignment vacantSlot,
        IReadOnlyList<Employee> candidates,
        IReadOnlyList<ShiftAssignment> sameDayAssignments,
        DeploymentPost? post)
    {
        var occupied = sameDayAssignments.Select(a => a.EmployeeId).ToHashSet();
        var list = new List<ReplacementCandidate>();
        foreach (var e in candidates.Where(x => x.Id != vacantSlot.EmployeeId && x.IsActive && !occupied.Contains(x.Id)))
        {
            var score = 0;
            var reasons = new List<string>();
            var rq = post?.RequiredQualificationCode;
            if (!string.IsNullOrEmpty(rq) &&
                e.Qualifications.Any(q => q.Qualification.Code == rq && (q.ValidUntil == null || q.ValidUntil >= vacantSlot.WorkDate)))
            {
                score += 80;
                reasons.Add("qualification match");
            }
            else if (!string.IsNullOrEmpty(rq))
            {
                score -= 100;
                reasons.Add("missing qualification");
            }

            if (post != null)
            {
                if (vacantSlot.ShiftTier.Code.Contains("Security", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(e.PrimaryRole, "Security", StringComparison.OrdinalIgnoreCase))
                {
                    score += post.StandardSecurityWeight;
                    reasons.Add("standard security priority");
                }
                else if (string.Equals(e.PrimaryRole, "LSKP", StringComparison.OrdinalIgnoreCase))
                {
                    score += post.LskpWeightOnSecurityPost;
                    reasons.Add("LSKP allowed with lower weight");
                }
            }

            list.Add(new ReplacementCandidate(e.Id, e.DisplayName, score, string.Join(", ", reasons)));
        }

        return [.. list.OrderByDescending(x => x.Score)];
    }
}
