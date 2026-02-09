using System.ComponentModel.DataAnnotations;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data.Entities;

public class AppUser : ISoftDeletable
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(150)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedUtc { get; set; }

    public ICollection<TenantUser> TenantUsers { get; set; } = new List<TenantUser>();
}
