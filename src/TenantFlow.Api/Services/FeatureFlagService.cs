using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TenantFlow.Api.Models;
using TenantFlow.Data;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly TenantFlowDbContext _db;
    private readonly IMemoryCache _cache;

    public FeatureFlagService(TenantFlowDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsEnabledAsync(Guid tenantId, string key, CancellationToken cancellationToken)
    {
        var cacheKey = $"ff:{tenantId}:{key}";
        if (_cache.TryGetValue(cacheKey, out bool cached))
        {
            return cached;
        }

        var flag = await _db.FeatureFlags
            .IgnoreQueryFilters()
            .AsNoTracking()
            .SingleOrDefaultAsync(f => f.TenantId == tenantId && f.Key == key && f.DeletedUtc == null, cancellationToken);

        var enabled = flag is { IsEnabled: true } && flag.RolloutPercent > 0;
        _cache.Set(cacheKey, enabled, TimeSpan.FromMinutes(2));
        return enabled;
    }

    public async Task<IReadOnlyList<FeatureFlagResponse>> GetForTenantAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var flags = await _db.FeatureFlags
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(f => f.TenantId == tenantId && f.DeletedUtc == null)
            .OrderBy(f => f.Key)
            .Select(f => new FeatureFlagResponse(f.Id, f.TenantId, f.Key, f.IsEnabled, f.RolloutPercent, f.UpdatedUtc))
            .ToListAsync(cancellationToken);

        return flags;
    }

    public async Task<FeatureFlagResponse> UpsertAsync(Guid tenantId, UpsertFeatureFlagRequest request, Guid? actorUserId, CancellationToken cancellationToken)
    {
        var normalizedKey = request.Key.Trim().ToLowerInvariant();
        var rollout = Math.Clamp(request.RolloutPercent, 0, 100);

        var existing = await _db.FeatureFlags
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(f => f.TenantId == tenantId && f.Key == normalizedKey, cancellationToken);

        if (existing is null)
        {
            existing = new FeatureFlag
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Key = normalizedKey,
                IsEnabled = request.IsEnabled,
                RolloutPercent = rollout,
                UpdatedUtc = DateTime.UtcNow
            };
            _db.FeatureFlags.Add(existing);
        }
        else
        {
            existing.IsEnabled = request.IsEnabled;
            existing.RolloutPercent = rollout;
            existing.UpdatedUtc = DateTime.UtcNow;
            existing.DeletedUtc = null;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _cache.Remove($"ff:{tenantId}:{normalizedKey}");

        return new FeatureFlagResponse(existing.Id, existing.TenantId, existing.Key, existing.IsEnabled, existing.RolloutPercent, existing.UpdatedUtc);
    }
}
