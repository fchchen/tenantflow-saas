using System.Security.Claims;
using TenantFlow.Data;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Api.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IMutableTenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var roles = context.User.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var userId = TryParseGuid(context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? context.User.FindFirstValue(ClaimTypes.Name)
                ?? context.User.FindFirstValue("sub"));

            var tenantId = TryParseGuid(context.User.FindFirstValue("tenant_id")) ?? Guid.Empty;

            var isPlatformAdmin = roles.Contains("PlatformAdmin", StringComparer.OrdinalIgnoreCase);
            if (isPlatformAdmin && context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue))
            {
                var overridden = TryParseGuid(headerValue.ToString());
                if (overridden.HasValue)
                {
                    tenantId = overridden.Value;
                }
            }

            tenantContext.Set(tenantId, userId, roles, true);
        }
        else
        {
            tenantContext.Set(Guid.Empty, null, Array.Empty<string>(), false);
        }

        await _next(context);
    }

    private static Guid? TryParseGuid(string? value)
    {
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
