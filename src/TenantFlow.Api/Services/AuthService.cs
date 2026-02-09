using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TenantFlow.Api.Models;
using TenantFlow.Data;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Services;

public class AuthService : IAuthService
{
    private readonly TenantFlowDbContext _db;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly PasswordHasher<AppUser> _passwordHasher = new();

    public AuthService(TenantFlowDbContext db, IJwtTokenService tokenService, Microsoft.Extensions.Options.IOptions<JwtSettings> jwtSettings)
    {
        _db = db;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var passwordVerification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordVerification == PasswordVerificationResult.Failed)
        {
            return null;
        }

        if (passwordVerification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var memberships = await _db.TenantUsers
            .Include(m => m.Tenant)
            .Where(m => m.UserId == user.Id)
            .ToListAsync(cancellationToken);

        if (memberships.Count == 0)
        {
            return null;
        }

        Guid tenantId;
        IReadOnlyCollection<string> roles;

        if (memberships.Any(m => m.Role == "PlatformAdmin"))
        {
            tenantId = DemoIds.PlatformTenantId;
            roles = memberships.Select(m => m.Role).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }
        else
        {
            var selectedMembership = memberships.First();

            if (!string.IsNullOrWhiteSpace(request.TenantSlug))
            {
                selectedMembership = memberships.FirstOrDefault(m => string.Equals(m.Tenant?.Slug, request.TenantSlug, StringComparison.OrdinalIgnoreCase))
                    ?? selectedMembership;
            }

            tenantId = selectedMembership.TenantId;
            roles = memberships
                .Where(m => m.TenantId == tenantId)
                .Select(m => m.Role)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var token = _tokenService.CreateToken(user.Id, user.Email, tenantId, roles);

        return new AuthResponse
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            TenantId = tenantId,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles
        };
    }

    public async Task<AuthResponse> DemoAsync(DemoLoginRequest request, CancellationToken cancellationToken)
    {
        var login = request.Persona.ToLowerInvariant() switch
        {
            "platform-admin" => new LoginRequest("platform@tenantflow.dev", SeedData.DemoPassword),
            "globex-admin" => new LoginRequest("admin@globex.dev", SeedData.DemoPassword),
            "tenant-user" => new LoginRequest("user@acme.dev", SeedData.DemoPassword),
            _ => new LoginRequest("admin@acme.dev", SeedData.DemoPassword)
        };

        var response = await LoginAsync(login, cancellationToken);
        return response ?? throw new InvalidOperationException("Demo identity is not configured.");
    }
}
