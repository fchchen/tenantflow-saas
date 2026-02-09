using TenantFlow.Api.Models;

namespace TenantFlow.Api.Services;

public interface IFeatureFlagService
{
    Task<bool> IsEnabledAsync(Guid tenantId, string key, CancellationToken cancellationToken);
    Task<IReadOnlyList<FeatureFlagResponse>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken);
    Task<FeatureFlagResponse> UpsertAsync(Guid tenantId, UpsertFeatureFlagRequest request, Guid? actorUserId, CancellationToken cancellationToken);
}
