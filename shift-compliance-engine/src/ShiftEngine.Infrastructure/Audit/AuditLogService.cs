using System.Text.Json;
using ShiftEngine.Domain.Entities;
using ShiftEngine.Infrastructure.Persistence;

namespace ShiftEngine.Infrastructure.Audit;

public sealed class AuditLogService(AppDbContext db)
{
    public async Task WriteAsync(
        Guid tenantId,
        string actorUserId,
        string action,
        string? entityType = null,
        string? entityId = null,
        object? details = null,
        CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLogEntry
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DetailsJson = details is null ? null : JsonSerializer.Serialize(details),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }
}
