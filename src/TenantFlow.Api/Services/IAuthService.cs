using TenantFlow.Api.Models;

namespace TenantFlow.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthResponse> DemoAsync(DemoLoginRequest request, CancellationToken cancellationToken);
}
