using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenantFlow.Api.Models;
using TenantFlow.Api.Services;
using TenantFlow.Data;
using TenantFlow.Data.Abstractions;
using TenantFlow.Data.Entities;

namespace TenantFlow.Api.Controllers;

[ApiController]
[Authorize(Policy = "PlatformAdmin")]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly TenantFlowDbContext _db;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IAuditService _audit;
    private readonly ITenantContext _tenantContext;

    public AdminController(TenantFlowDbContext db, IFeatureFlagService featureFlags, IAuditService audit, ITenantContext tenantContext)
    {
        _db = db;
        _featureFlags = featureFlags;
        _audit = audit;
        _tenantContext = tenantContext;
    }

    [HttpGet("tenants")]
    public async Task<ActionResult<IReadOnlyList<TenantResponse>>> GetTenants(CancellationToken cancellationToken)
    {
        var rows = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.DeletedUtc == null)
            .OrderBy(t => t.Slug)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.Slug,
                t.IsActive,
                UserCount = t.TenantUsers.Count
            })
            .ToListAsync(cancellationToken);

        var tenants = rows
            .Select(t => new TenantResponse(t.Id, t.Name, t.Slug, t.IsActive, t.UserCount))
            .ToList();

        return Ok(tenants);
    }

    [HttpPost("tenants")]
    public async Task<ActionResult<TenantResponse>> CreateTenant([FromBody] CreateTenantRequest request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var exists = await _db.Tenants.AnyAsync(t => t.Slug == slug && t.DeletedUtc == null, cancellationToken);
        if (exists)
        {
            return Conflict(new { message = "Tenant slug already exists." });
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            IsActive = true,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync(cancellationToken);

        var metadata = JsonSerializer.Serialize(new { slug = tenant.Slug });
        await _audit.LogAsync(DemoIds.PlatformTenantId, _tenantContext.UserId, "tenant.create", nameof(Tenant), tenant.Id.ToString(), metadata, cancellationToken);

        return CreatedAtAction(nameof(GetTenants), new { id = tenant.Id }, new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.IsActive, 0));
    }

    [HttpGet("usage")]
    public async Task<ActionResult<IReadOnlyList<UsageSummaryResponse>>> GetUsage(CancellationToken cancellationToken)
    {
        var usageRows = await _db.UsageEvents
            .AsNoTracking()
            .GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Total = g.Sum(x => x.Quantity) })
            .ToListAsync(cancellationToken);

        var slugMap = await _db.Tenants
            .AsNoTracking()
            .Where(t => t.DeletedUtc == null)
            .ToDictionaryAsync(t => t.Id, t => t.Slug, cancellationToken);

        var usage = usageRows
            .Select(u => new UsageSummaryResponse(
                u.TenantId,
                slugMap.GetValueOrDefault(u.TenantId, "unknown"),
                u.Total))
            .OrderByDescending(u => u.TotalQuantity)
            .ToList();

        return Ok(usage);
    }

    [HttpGet("feature-flags/{tenantId:guid}")]
    public async Task<ActionResult<IReadOnlyList<FeatureFlagResponse>>> GetFeatureFlags([FromRoute] Guid tenantId, CancellationToken cancellationToken)
    {
        var flags = await _featureFlags.GetForTenantAsync(tenantId, cancellationToken);
        return Ok(flags);
    }

    [HttpPut("feature-flags/{tenantId:guid}")]
    public async Task<ActionResult<FeatureFlagResponse>> UpsertFeatureFlag([FromRoute] Guid tenantId, [FromBody] UpsertFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        var response = await _featureFlags.UpsertAsync(tenantId, request, _tenantContext.UserId, cancellationToken);
        var metadata = JsonSerializer.Serialize(new { tenantId, key = response.Key });
        await _audit.LogAsync(DemoIds.PlatformTenantId, _tenantContext.UserId, "feature-flag.upsert", nameof(FeatureFlag), response.Id.ToString(), metadata, cancellationToken);

        return Ok(response);
    }
}
