namespace TenantFlow.Data.Abstractions;

public interface ITenantContext
{
    Guid TenantId { get; }
    Guid? UserId { get; }
    bool IsAuthenticated { get; }
    bool IsPlatformAdmin { get; }
    IReadOnlyCollection<string> Roles { get; }
}
