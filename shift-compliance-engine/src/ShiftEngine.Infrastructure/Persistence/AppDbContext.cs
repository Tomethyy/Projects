using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShiftEngine.Domain.Common;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Identity;

namespace ShiftEngine.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<EmployeeQualification> EmployeeQualifications => Set<EmployeeQualification>();
    public DbSet<ShiftTier> ShiftTiers => Set<ShiftTier>();
    public DbSet<RosterPeriod> RosterPeriods => Set<RosterPeriod>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();
    public DbSet<LeaveRecord> LeaveRecords => Set<LeaveRecord>();
    public DbSet<DeploymentPost> DeploymentPosts => Set<DeploymentPost>();
    public DbSet<DailyLedgerEntry> DailyLedgerEntries => Set<DailyLedgerEntry>();
    public DbSet<SickReplanProposal> SickReplanProposals => Set<SickReplanProposal>();
    public DbSet<ShiftSwap> ShiftSwaps => Set<ShiftSwap>();
    public DbSet<TrainingEvent> TrainingEvents => Set<TrainingEvent>();
    public DbSet<AuditLogEntry> AuditLogs => Set<AuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Tenant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Slug).IsUnique();
        });
        b.Entity<AppUser>(e => { e.HasIndex(x => new { x.TenantId, x.NormalizedEmail }); });
        b.Entity<Employee>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TenantId, x.PersonnelNumber }).IsUnique();
            e.HasMany(x => x.Qualifications).WithOne(x => x.Employee).HasForeignKey(x => x.EmployeeId);
        });
        b.Entity<Qualification>(e => { e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique(); });
        b.Entity<EmployeeQualification>(e =>
        {
            e.HasOne(x => x.Employee).WithMany(x => x.Qualifications).HasForeignKey(x => x.EmployeeId);
            e.HasOne(x => x.Qualification).WithMany().HasForeignKey(x => x.QualificationId);
        });
        b.Entity<ShiftTier>(e => { e.HasIndex(x => new { x.TenantId, x.Code }).IsUnique(); });
        b.Entity<RosterPeriod>(e =>
        {
            e.HasMany(x => x.Assignments).WithOne(x => x.RosterPeriod).HasForeignKey(x => x.RosterPeriodId);
        });
        b.Entity<ShiftAssignment>(e =>
        {
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
            e.HasOne(x => x.ShiftTier).WithMany().HasForeignKey(x => x.ShiftTierId);
            e.HasOne(x => x.DeploymentPost).WithMany().HasForeignKey(x => x.DeploymentPostId);
            e.HasIndex(x => new { x.TenantId, x.RosterPeriodId, x.WorkDate, x.EmployeeId });
        });
        b.Entity<LeaveRecord>(e => { e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId); });
        b.Entity<DeploymentPost>(e => { e.HasIndex(x => new { x.TenantId, x.Name }); });
        b.Entity<DailyLedgerEntry>(e =>
        {
            e.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId);
            e.HasOne(x => x.ShiftAssignment).WithMany().HasForeignKey(x => x.ShiftAssignmentId);
        });
        b.Entity<SickReplanProposal>(e => { e.HasOne(x => x.LedgerEntry).WithMany().HasForeignKey(x => x.LedgerEntryId); });
        b.Entity<ShiftSwap>(e =>
        {
            e.HasOne(x => x.RequesterEmployee).WithMany().HasForeignKey(x => x.RequesterEmployeeId);
            e.HasOne(x => x.TargetEmployee).WithMany().HasForeignKey(x => x.TargetEmployeeId);
            e.HasOne(x => x.RequesterAssignment).WithMany().HasForeignKey(x => x.RequesterAssignmentId);
            e.HasOne(x => x.TargetAssignment).WithMany().HasForeignKey(x => x.TargetAssignmentId);
        });
        foreach (var et in b.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(et.ClrType))
            {
                b.Entity(et.ClrType).HasIndex(nameof(ITenantEntity.TenantId));
            }
        }
    }
}
