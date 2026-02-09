namespace TenantFlow.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(Guid userId, string email, Guid tenantId, IReadOnlyCollection<string> roles);
}
