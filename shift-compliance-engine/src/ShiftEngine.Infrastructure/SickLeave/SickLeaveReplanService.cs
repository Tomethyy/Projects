using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;
using ShiftEngine.Replanning;

namespace ShiftEngine.Infrastructure.SickLeave;

public class SickLeaveReplanService(AppDbContext db)
{
    public async Task<SickReplanProposal> ProposeAsync(Guid tenantId, Guid ledgerEntryId, CancellationToken ct = default)
    {
        var entry = await db.DailyLedgerEntries
            .Include(e => e.Employee)
            .Include(e => e.ShiftAssignment)!.ThenInclude(a => a!.ShiftTier)
            .Include(e => e.ShiftAssignment)!.ThenInclude(a => a!.DeploymentPost)
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == ledgerEntryId, ct)
            ?? throw new InvalidOperationException("Ledger entry not found");
        if (entry.Kind is not LedgerEntryKind.SickLeave and not LedgerEntryKind.CallOut)
            throw new InvalidOperationException("Not a sick/call-out entry");
        var assignment = entry.ShiftAssignment ?? throw new InvalidOperationException("No shift assignment linked");
        var post = assignment.DeploymentPost;
        var sameDay = await db.ShiftAssignments
            .Include(a => a.ShiftTier)
            .Where(a => a.TenantId == tenantId && a.WorkDate == assignment.WorkDate && a.RosterPeriodId == assignment.RosterPeriodId)
            .ToListAsync(ct);
        var candidates = await db.Employees
            .Include(e => e.Qualifications).ThenInclude(q => q.Qualification)
            .Where(e => e.TenantId == tenantId && e.IsActive)
            .ToListAsync(ct);
        var ranked = SickLeaveCandidateRanker.Rank(assignment, candidates, sameDay, post);
        var payload = JsonSerializer.Serialize(ranked);
        var proposal = new SickReplanProposal
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            LedgerEntryId = ledgerEntryId,
            JsonPayload = payload,
            IsApplied = false
        };
        db.SickReplanProposals.Add(proposal);
        await db.SaveChangesAsync(ct);
        return proposal;
    }

    public async Task ApplyAsync(Guid tenantId, Guid proposalId, Guid replacementEmployeeId, CancellationToken ct = default)
    {
        var proposal = await db.SickReplanProposals
            .Include(p => p.LedgerEntry).ThenInclude(l => l.ShiftAssignment)
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == proposalId, ct)
            ?? throw new InvalidOperationException("Proposal not found");
        if (proposal.IsApplied) return;
        var assignment = proposal.LedgerEntry.ShiftAssignment ?? throw new InvalidOperationException("No assignment");
        assignment.EmployeeId = replacementEmployeeId;
        proposal.IsApplied = true;
        await db.SaveChangesAsync(ct);
    }
}
