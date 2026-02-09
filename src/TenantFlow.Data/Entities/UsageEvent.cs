using System.ComponentModel.DataAnnotations;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data.Entities;

public class UsageEvent : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    [MaxLength(1000)]
    public string? MetadataJson { get; set; }

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}
