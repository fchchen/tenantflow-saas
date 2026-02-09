using TenantFlow.Data.Abstractions;

namespace TenantFlow.Data;

public class TenantContext : IMutableTenantContext
{
    public Guid TenantId { get; private set; } = Guid.Empty;
    public Guid? UserId { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public IReadOnlyCollection<string> Roles { get; private set; } = Array.Empty<string>();
    public bool IsPlatformAdmin => Roles.Contains("PlatformAdmin", StringComparer.OrdinalIgnoreCase);

    public void Set(Guid tenantId, Guid? userId, IReadOnlyCollection<string> roles, bool isAuthenticated)
    {
        TenantId = tenantId;
        UserId = userId;
        Roles = roles;
        IsAuthenticated = isAuthenticated;
    }
}
