using TenantFlow.Data;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Services;

public class AuditService : IAuditService
{
    private readonly TenantFlowDbContext _db;

    public AuditService(TenantFlowDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(Guid tenantId, Guid? actorUserId, string action, string entityType, string entityId, string? diffJson, CancellationToken cancellationToken)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DiffJson = diffJson,
            OccurredUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
