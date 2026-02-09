namespace TenantFlow.Data.Abstractions;

public interface IMutableTenantContext : ITenantContext
{
    void Set(Guid tenantId, Guid? userId, IReadOnlyCollection<string> roles, bool isAuthenticated);
}
