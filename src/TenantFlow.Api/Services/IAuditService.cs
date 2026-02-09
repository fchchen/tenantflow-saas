namespace TenantFlow.Api.Services;

public interface IAuditService
{
    Task LogAsync(Guid tenantId, Guid? actorUserId, string action, string entityType, string entityId, string? diffJson, CancellationToken cancellationToken);
}
