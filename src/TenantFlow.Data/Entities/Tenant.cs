using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Data.Entities;

public class Tenant
{
    public Guid Id { get; set; }

    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(80)]
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedUtc { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
    public ICollection<FeatureFlag> FeatureFlags { get; set; } = new List<FeatureFlag>();
}
