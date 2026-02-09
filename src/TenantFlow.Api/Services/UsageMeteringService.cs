using TenantFlow.Data;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Services;

public class UsageMeteringService : IUsageMeteringService
{
    private readonly TenantFlowDbContext _db;

    public UsageMeteringService(TenantFlowDbContext db)
    {
        _db = db;
    }

    public async Task TrackAsync(Guid tenantId, string eventType, int quantity, string? metadataJson, CancellationToken cancellationToken)
    {
        _db.UsageEvents.Add(new UsageEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventType = eventType,
            Quantity = quantity,
            MetadataJson = metadataJson,
            OccurredUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
