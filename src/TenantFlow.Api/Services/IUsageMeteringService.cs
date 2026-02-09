namespace TenantFlow.Api.Services;

public interface IUsageMeteringService
{
    Task TrackAsync(Guid tenantId, string eventType, int quantity, string? metadataJson, CancellationToken cancellationToken);
}
