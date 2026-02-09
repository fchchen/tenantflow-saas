using System.ComponentModel.DataAnnotations;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data.Entities;

public class FeatureFlag : ITenantScoped, ISoftDeletable
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    [MaxLength(120)]
    public string Key { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }
    public int RolloutPercent { get; set; } = 100;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedUtc { get; set; }

    public Tenant? Tenant { get; set; }
}
