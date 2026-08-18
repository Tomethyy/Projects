using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Api.Models;
using ShiftEngine.Application.Rostering;
using ShiftEngine.Domain;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Domain.Rostering;
using ShiftEngine.Infrastructure.Audit;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RosterController(AppDbContext db, AuditLogService audit) : ControllerBase
{
    public record GenerateYearRequest(
        Guid EmployeeId,
        int Year,
        DateOnly AnchorFirstWorkDay,
        Guid? ShiftTierId,
        string? LegacySource,
        /// <summary>Optional legacy override; omit for automatic 6/2–6/3 rhythm from <see cref="Employee.ContractedHoursMonthly"/>.</summary>
        RosterPatternKind? Pattern = null);

    public record PublishRequest(Guid RosterPeriodId);

    public record GenerateTeamMonthRequest(
        int Year,
        int Month,
        DateOnly? AnchorFirstWorkDay,
        Guid? ShiftTierId,
        string? LegacySource,
        bool ReplaceExisting = true,
        /// <summary>When true (default), offset each employee's rotation anchor across the team cycle.</summary>
        bool StaggerTeamAnchors = true,
        /// <summary>When true (default), assign deployment posts and shift tiers after rotation planning.</summary>
        bool AssignPosts = true);

    [HttpGet("periods")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<RosterPeriodSummary>>> ListPeriods(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var q = db.RosterPeriods.Where(p => p.TenantId == tenantId);
        if (year is not null)
            q = q.Where(p => p.StartDate.Year == year);
        if (month is not null)
            q = q.Where(p => p.StartDate.Month == month);
        var list = await q
            .OrderByDescending(p => p.StartDate)
            .Select(p => new RosterPeriodSummary(p.Id, p.Name, p.StartDate, p.EndDate, p.IsPublished))
            .AsNoTracking()
            .ToListAsync(ct);
        return list;
    }

    [HttpPost("generate-team-month")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<object>> GenerateTeamMonth([FromBody] GenerateTeamMonthRequest req, CancellationToken ct)
    {
        if (req.Month is < 1 or > 12) return BadRequest("Month must be 1–12.");
        var tenantId = User.GetTenantId();
        var (start, end) = TeamMonthRosterBuilder.GetMonthBounds(req.Year, req.Month);
        var anchor = req.AnchorFirstWorkDay ?? TeamMonthRosterBuilder.DefaultAnchor(req.Year, req.Month);
        var periodName = TeamMonthRosterBuilder.FormatPeriodName(req.Year, req.Month);

        var period = await db.RosterPeriods.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.StartDate == start && p.EndDate == end && p.Name == periodName,
            ct);

        if (period is null)
        {
            period = new RosterPeriod
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = periodName,
                StartDate = start,
                EndDate = end,
                LegacySource = string.IsNullOrWhiteSpace(req.LegacySource) ? "Synthetic" : req.LegacySource.Trim(),
                LegacyReferenceMode = LegacyReferenceMode.ShadowPlanning
            };
            db.RosterPeriods.Add(period);
        }
        else if (req.ReplaceExisting)
        {
            var existing = await db.ShiftAssignments.Where(a => a.RosterPeriodId == period.Id).ToListAsync(ct);
            db.ShiftAssignments.RemoveRange(existing);
        }
        else if (await db.ShiftAssignments.AnyAsync(a => a.RosterPeriodId == period.Id, ct))
            return Conflict("Team month already has assignments; set replaceExisting=true to regenerate.");

        var tierId = req.ShiftTierId ?? await db.ShiftTiers
            .Where(t => t.TenantId == tenantId && t.Code == "EARLY")
            .Select(t => t.Id)
            .FirstAsync(ct);

        var employees = await db.Employees
            .Where(e => e.TenantId == tenantId && e.IsActive)
            .OrderBy(e => e.PersonnelNumber)
            .ToListAsync(ct);
        if (employees.Count == 0) return BadRequest("No active employees.");

        var pending = new List<ShiftAssignment>();
        for (var i = 0; i < employees.Count; i++)
        {
            var emp = employees[i];
            var pattern = RosterRotationPlanner.ResolvePattern(emp.ContractedHoursMonthly, null);
            var cycleLen = RosterRotationPlanner.BuildCycle(pattern, emp.ContractedHoursMonthly).Count;
            var empAnchor = req.StaggerTeamAnchors
                ? TeamMonthRosterBuilder.StaggeredAnchor(anchor, i, employees.Count, cycleLen)
                : anchor;
            foreach (var d in TeamMonthRosterBuilder.EnumerateWorkDaysInMonth(
                         req.Year,
                         req.Month,
                         emp.ContractedHoursMonthly,
                         empAnchor))
            {
                pending.Add(new ShiftAssignment
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    RosterPeriodId = period.Id,
                    EmployeeId = emp.Id,
                    ShiftTierId = tierId,
                    WorkDate = d
                });
            }
        }

        PostAssignmentResult? postResult = null;
        if (req.AssignPosts && pending.Count > 0)
        {
            var posts = await db.DeploymentPosts.Where(p => p.TenantId == tenantId).ToListAsync(ct);
            if (posts.Count == 0)
                return BadRequest("No deployment posts. Import positions before generating the team month.");
            var tiers = await db.ShiftTiers.Where(t => t.TenantId == tenantId).ToListAsync(ct);
            var tiersByCode = tiers.ToDictionary(t => t.Code, StringComparer.OrdinalIgnoreCase);
            postResult = PostAssignmentPlanner.Assign(
                pending,
                employees.ToDictionary(e => e.Id),
                posts,
                tiersByCode);
        }

        db.ShiftAssignments.AddRange(pending);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            tenantId,
            User.GetUserId(),
            "Roster.GenerateTeamMonth",
            nameof(RosterPeriod),
            period.Id.ToString(),
            new
            {
                req.Year,
                req.Month,
                employeeCount = employees.Count,
                assignmentCount = pending.Count,
                postResult?.SlotsFilled,
                postResult?.SlotsUnfilled
            },
            ct);
        return Ok(new
        {
            period.Id,
            period.Name,
            req.Year,
            req.Month,
            employeeCount = employees.Count,
            assignmentCount = pending.Count,
            anchorFirstWorkDay = anchor,
            postAssignment = postResult
        });
    }

    [HttpPost("assign-posts")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<PostAssignmentResult>> AssignPosts(
        [FromQuery] Guid periodId,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var period = await db.RosterPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (period is null) return NotFound();

        var assignments = await db.ShiftAssignments
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId)
            .ToListAsync(ct);
        if (assignments.Count == 0) return BadRequest("No assignments in this period.");

        var employees = await db.Employees.Where(e => e.TenantId == tenantId && e.IsActive).ToListAsync(ct);
        var posts = await db.DeploymentPosts.Where(p => p.TenantId == tenantId).ToListAsync(ct);
        if (posts.Count == 0) return BadRequest("No deployment posts.");

        var tiers = await db.ShiftTiers.Where(t => t.TenantId == tenantId).ToListAsync(ct);
        var result = PostAssignmentPlanner.Assign(
            assignments,
            employees.ToDictionary(e => e.Id),
            posts,
            tiers.ToDictionary(t => t.Code, StringComparer.OrdinalIgnoreCase));

        await db.SaveChangesAsync(ct);
        return result;
    }

    [HttpGet("matrix")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<RosterMatrixResponse>> Matrix(
        [FromQuery] int? year,
        [FromQuery] int? month,
        [FromQuery] Guid? periodId,
        CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        RosterPeriod? period;
        if (periodId is not null)
        {
            period = await db.RosterPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        }
        else if (year is not null && month is not null)
        {
            var (start, end) = TeamMonthRosterBuilder.GetMonthBounds(year.Value, month.Value);
            var periodName = TeamMonthRosterBuilder.FormatPeriodName(year.Value, month.Value);
            period = await db.RosterPeriods.FirstOrDefaultAsync(
                p => p.TenantId == tenantId && p.StartDate == start && p.EndDate == end && p.Name == periodName,
                ct);
        }
        else
            return BadRequest("Provide periodId or year and month.");

        if (period is null) return NotFound("No roster period for this month. Generate the team plan first.");

        var y = period.StartDate.Year;
        var m = period.StartDate.Month;
        var (rangeStart, rangeEnd) = TeamMonthRosterBuilder.GetMonthBounds(y, m);
        var days = Enumerable.Range(0, rangeEnd.DayNumber - rangeStart.DayNumber + 1)
            .Select(i => rangeStart.AddDays(i))
            .ToList();

        var employees = await db.Employees
            .Where(e => e.TenantId == tenantId && e.IsActive)
            .OrderBy(e => e.PersonnelNumber)
            .Select(e => new RosterMatrixEmployeeRow(e.Id, e.PersonnelNumber, e.DisplayName, e.ContractedHoursMonthly))
            .AsNoTracking()
            .ToListAsync(ct);

        var assignments = await db.ShiftAssignments
            .Include(a => a.ShiftTier)
            .Include(a => a.DeploymentPost)
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == period.Id)
            .AsNoTracking()
            .ToListAsync(ct);

        var cells = assignments
            .Select(a => new RosterMatrixCell(
                a.EmployeeId,
                a.WorkDate,
                a.Id,
                a.ShiftTierId,
                a.ShiftTier.Code,
                a.ShiftTier.DisplayName,
                a.DeploymentPostId,
                a.DeploymentPost?.Name))
            .ToList();

        return new RosterMatrixResponse(
            period.Id,
            y,
            m,
            period.Name,
            period.IsPublished,
            days,
            employees,
            cells);
    }

    [HttpGet("deployment-grid")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<DeploymentGridResponse>> DeploymentGrid(
        [FromQuery] int? year,
        [FromQuery] int? month,
        CancellationToken ct)
    {
        if (year is null || month is null) return BadRequest("year and month required.");
        var tenantId = User.GetTenantId();
        var (start, end) = TeamMonthRosterBuilder.GetMonthBounds(year.Value, month.Value);
        var periodName = TeamMonthRosterBuilder.FormatPeriodName(year.Value, month.Value);
        var period = await db.RosterPeriods.AsNoTracking()
            .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.StartDate == start && p.EndDate == end && p.Name == periodName, ct);
        if (period is null) return NotFound("No roster period for this month.");

        var days = Enumerable.Range(0, end.DayNumber - start.DayNumber + 1)
            .Select(i => start.AddDays(i)).ToList();
        var posts = await db.DeploymentPosts.Where(p => p.TenantId == tenantId).OrderBy(p => p.Name).AsNoTracking().ToListAsync(ct);
        var assigns = await db.ShiftAssignments
            .Include(a => a.Employee).Include(a => a.ShiftTier).Include(a => a.DeploymentPost)
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == period.Id && a.DeploymentPostId != null)
            .AsNoTracking().ToListAsync(ct);

        var cells = posts.SelectMany(post => days.Select(day =>
        {
            var filled = assigns
                .Where(a => a.DeploymentPostId == post.Id && a.WorkDate == day)
                .Select(a => new DeploymentGridSlot(
                    a.Employee.PersonnelNumber,
                    a.Employee.DisplayName,
                    a.ShiftTier.Code))
                .ToList();
            return new DeploymentGridCell(post.Id, post.Name, day, post.RequiredHeadcount, filled);
        })).ToList();

        return new DeploymentGridResponse(period.Id, year.Value, month.Value, period.IsPublished, days, cells);
    }

    [HttpPost("periods")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult<Guid>> CreatePeriod([FromBody] RosterPeriod period, CancellationToken ct)
    {
        period.Id = Guid.NewGuid();
        period.TenantId = User.GetTenantId();
        period.IsPublished = false;
        db.RosterPeriods.Add(period);
        await db.SaveChangesAsync(ct);
        return period.Id;
    }

    [HttpPost("generate-year")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult> GenerateYear([FromBody] GenerateYearRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var emp = await db.Employees.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == req.EmployeeId, ct);
        if (emp == null) return NotFound();
        var tierId = req.ShiftTierId ?? await db.ShiftTiers.Where(t => t.TenantId == tenantId && t.Code == "EARLY").Select(t => t.Id).FirstAsync(ct);
        var tier = await db.ShiftTiers.FirstAsync(t => t.Id == tierId, ct);
        var pattern = RosterRotationPlanner.ResolvePattern(emp.ContractedHoursMonthly, req.Pattern);
        var hoursPerShift = RosterRotationPlanner.EstimateHoursPerShift(tier.StartLocal, tier.EndLocal);
        var start = new DateOnly(req.Year, 1, 1);
        var end = new DateOnly(req.Year, 12, 31);
        var period = new RosterPeriod
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = $"{req.Year} {emp.DisplayName}",
            StartDate = start,
            EndDate = end,
            LegacySource = string.IsNullOrWhiteSpace(req.LegacySource) ? "Synthetic" : req.LegacySource.Trim(),
            LegacyReferenceMode = LegacyReferenceMode.ShadowPlanning
        };
        db.RosterPeriods.Add(period);
        var workDayCount = 0;
        foreach (var (d, work) in RosterRotationPlanner.EnumerateYear(start, end, pattern, emp.ContractedHoursMonthly, req.AnchorFirstWorkDay))
        {
            if (!work) continue;
            workDayCount++;
            db.ShiftAssignments.Add(new ShiftAssignment
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                RosterPeriodId = period.Id,
                EmployeeId = emp.Id,
                ShiftTierId = tier.Id,
                WorkDate = d
            });
        }

        await db.SaveChangesAsync(ct);
        var cycle = RosterRotationPlanner.BuildCycle(pattern, emp.ContractedHoursMonthly);
        return Ok(new
        {
            period.Id,
            patternApplied = pattern.ToString(),
            rhythmDescription = RosterRotationPlanner.DescribeCycle(cycle),
            contractedHoursMonthly = emp.ContractedHoursMonthly,
            referenceHoursMonthly = RosterRotationPlanner.ReferenceHours,
            hoursPerShift,
            workDaysInYear = workDayCount,
            estimatedAnnualHours = workDayCount * hoursPerShift
        });
    }

    [HttpPost("publish")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<ActionResult> Publish([FromBody] PublishRequest req, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var p = await db.RosterPeriods.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == req.RosterPeriodId, ct);
        if (p == null) return NotFound();
        p.IsPublished = true;
        p.PublishedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            tenantId,
            User.GetUserId(),
            "Roster.Publish",
            nameof(RosterPeriod),
            p.Id.ToString(),
            new { p.Name, p.StartDate, p.EndDate },
            ct);
        return Ok();
    }

    [HttpGet("assignments")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<ShiftAssignment>>> Assignments([FromQuery] Guid periodId, CancellationToken ct) =>
        await db.ShiftAssignments.Include(a => a.Employee).Include(a => a.ShiftTier)
            .Where(a => a.TenantId == User.GetTenantId() && a.RosterPeriodId == periodId)
            .AsNoTracking().ToListAsync(ct);

    [HttpGet("shift-tiers")]
    [Authorize(Roles = SecurityRoles.OperationsReaders)]
    public async Task<ActionResult<List<ShiftTier>>> ShiftTiers(CancellationToken ct) =>
        await db.ShiftTiers.Where(t => t.TenantId == User.GetTenantId()).OrderBy(t => t.Code).AsNoTracking().ToListAsync(ct);

    public record PatchAssignmentTierRequest(Guid ShiftTierId);

    public record TierBulkUpdate(Guid AssignmentId, Guid ShiftTierId);

    public record BulkTierPatchRequest(IReadOnlyList<TierBulkUpdate> Updates);

    [HttpPatch("assignments/tiers/bulk")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> BulkPatchAssignmentTiers([FromBody] BulkTierPatchRequest body, CancellationToken ct)
    {
        if (body.Updates.Count == 0) return BadRequest("No updates.");
        var tenantId = User.GetTenantId();
        var ids = body.Updates.Select(u => u.AssignmentId).ToHashSet();
        var assigns = await db.ShiftAssignments
            .Include(a => a.RosterPeriod)
            .Where(a => a.TenantId == tenantId && ids.Contains(a.Id))
            .ToListAsync(ct);
        if (assigns.Count == 0) return NotFound();
        if (assigns.Any(a => a.RosterPeriod.IsPublished))
            return Conflict("Published periods cannot be edited.");

        var tierIds = body.Updates.Select(u => u.ShiftTierId).Distinct().ToList();
        var validTierCount = await db.ShiftTiers.CountAsync(t => t.TenantId == tenantId && tierIds.Contains(t.Id), ct);
        if (validTierCount != tierIds.Count) return BadRequest("Unknown shift tier.");

        var byId = body.Updates.ToDictionary(u => u.AssignmentId, u => u.ShiftTierId);
        foreach (var a in assigns.Where(a => byId.ContainsKey(a.Id)))
            a.ShiftTierId = byId[a.Id];

        await db.SaveChangesAsync(ct);
        return Ok(new { updated = assigns.Count });
    }

    [HttpPatch("assignments/{assignmentId:guid}/tier")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> PatchAssignmentTier(Guid assignmentId, [FromBody] PatchAssignmentTierRequest body, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var a = await db.ShiftAssignments.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == assignmentId, ct);
        if (a == null) return NotFound();
        var tierOk = await db.ShiftTiers.AnyAsync(t => t.TenantId == tenantId && t.Id == body.ShiftTierId, ct);
        if (!tierOk) return BadRequest("Unknown shift tier.");
        a.ShiftTierId = body.ShiftTierId;
        await db.SaveChangesAsync(ct);
        return Ok();
    }

    [HttpDelete("periods/{periodId:guid}")]
    [Authorize(Roles = SecurityRoles.OperationsWriters)]
    public async Task<IActionResult> DeletePeriod(Guid periodId, CancellationToken ct)
    {
        var tenantId = User.GetTenantId();
        var period = await db.RosterPeriods.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == periodId, ct);
        if (period is null) return NotFound();
        if (period.IsPublished)
            return Conflict("Cannot delete a published roster period.");

        var assignmentIds = await db.ShiftAssignments
            .Where(a => a.TenantId == tenantId && a.RosterPeriodId == periodId)
            .Select(a => a.Id)
            .ToListAsync(ct);

        if (assignmentIds.Count > 0)
        {
            var ledgerIds = await db.DailyLedgerEntries
                .Where(e => e.ShiftAssignmentId != null && assignmentIds.Contains(e.ShiftAssignmentId.Value))
                .Select(e => e.Id)
                .ToListAsync(ct);
            if (ledgerIds.Count > 0)
            {
                var proposals = await db.SickReplanProposals.Where(p => ledgerIds.Contains(p.LedgerEntryId)).ToListAsync(ct);
                db.SickReplanProposals.RemoveRange(proposals);
            }

            var ledgerEntries = await db.DailyLedgerEntries
                .Where(e => e.ShiftAssignmentId != null && assignmentIds.Contains(e.ShiftAssignmentId.Value))
                .ToListAsync(ct);
            foreach (var entry in ledgerEntries)
                entry.ShiftAssignmentId = null;
        }

        db.RosterPeriods.Remove(period);
        await db.SaveChangesAsync(ct);
        await audit.WriteAsync(
            tenantId,
            User.GetUserId(),
            "Roster.DeletePeriod",
            nameof(RosterPeriod),
            periodId.ToString(),
            new { period.Name, period.StartDate, period.EndDate },
            ct);
        return NoContent();
    }
}
