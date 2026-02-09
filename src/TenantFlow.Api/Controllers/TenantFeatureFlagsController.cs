using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantFlow.Api.Models;
using TenantFlow.Api.Services;
using TenantFlow.Data.Abstractions;

namespace TenantFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/tenant/feature-flags")]
public class TenantFeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;
    private readonly ITenantContext _tenantContext;

    public TenantFeatureFlagsController(IFeatureFlagService featureFlags, ITenantContext tenantContext)
    {
        _featureFlags = featureFlags;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FeatureFlagResponse>>> Get(CancellationToken cancellationToken)
    {
        var flags = await _featureFlags.GetForTenantAsync(_tenantContext.TenantId, cancellationToken);
        return Ok(flags);
    }
}
