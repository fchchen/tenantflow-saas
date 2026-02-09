using System.ComponentModel.DataAnnotations;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data.Entities;

public class AuditLog : ITenantScoped
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ActorUserId { get; set; }

    [MaxLength(60)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(80)]
    public string EntityType { get; set; } = string.Empty;

    [MaxLength(80)]
    public string EntityId { get; set; } = string.Empty;

    [MaxLength(3000)]
    public string? DiffJson { get; set; }

    public DateTime OccurredUtc { get; set; } = DateTime.UtcNow;
}
