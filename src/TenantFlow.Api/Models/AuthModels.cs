using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Api.Models;

public sealed record LoginRequest(
    [param: Required, EmailAddress, MaxLength(200)] string Email,
    [param: Required, MinLength(8), MaxLength(128)] string Password,
    [param: MaxLength(80), RegularExpression("^[a-z0-9-]+$")] string? TenantSlug = null);

public sealed record DemoLoginRequest(
    [param: Required, RegularExpression("^(platform-admin|tenant-admin|tenant-user|globex-admin)$")] string Persona = "tenant-admin");

public sealed class AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public DateTime ExpiresAtUtc { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
}

public sealed class JwtSettings
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "TenantFlow.Api";
    public string Audience { get; set; } = "TenantFlow.Ui";
    public int ExpirationMinutes { get; set; } = 60;
}
