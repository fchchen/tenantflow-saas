using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Data.Entities;

public class TenantUser
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }

    [MaxLength(40)]
    public string Role { get; set; } = string.Empty;

    public DateTime JoinedUtc { get; set; } = DateTime.UtcNow;

    public Tenant? Tenant { get; set; }
    public AppUser? User { get; set; }
}
