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
[Authorize]
[Route("api/v1/quotes")]
public class QuotesController : ControllerBase
{
    private readonly TenantFlowDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IAuditService _audit;
    private readonly IUsageMeteringService _metering;

    public QuotesController(
        TenantFlowDbContext db,
        ITenantContext tenantContext,
        IFeatureFlagService featureFlags,
        IAuditService audit,
        IUsageMeteringService metering)
    {
        _db = db;
        _tenantContext = tenantContext;
        _featureFlags = featureFlags;
        _audit = audit;
        _metering = metering;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<QuoteResponse>>> Get(CancellationToken cancellationToken)
    {
        var quotes = await _db.Quotes
            .AsNoTracking()
            .OrderByDescending(q => q.CreatedUtc)
            .Select(q => new QuoteResponse(q.Id, q.QuoteNumber, q.CustomerName, q.Premium, q.CreatedUtc))
            .ToListAsync(cancellationToken);

        return Ok(quotes);
    }

    [HttpPost]
    public async Task<ActionResult<QuoteResponse>> Create([FromBody] CreateQuoteRequest request, CancellationToken cancellationToken)
    {
        var enabled = await _featureFlags.IsEnabledAsync(_tenantContext.TenantId, "quote.create", cancellationToken);
        if (!enabled)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Feature flag quote.create is disabled." });
        }

        var quote = new Quote
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            QuoteNumber = $"Q-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100, 999)}",
            CustomerName = request.CustomerName.Trim(),
            Premium = request.Premium,
            CreatedUtc = DateTime.UtcNow
        };

        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync(cancellationToken);

        var metadata = JsonSerializer.Serialize(new { quoteId = quote.Id });
        await _metering.TrackAsync(_tenantContext.TenantId, "quote.created", 1, metadata, cancellationToken);
        await _audit.LogAsync(_tenantContext.TenantId, _tenantContext.UserId, "quote.create", nameof(Quote), quote.Id.ToString(), null, cancellationToken);

        var response = new QuoteResponse(quote.Id, quote.QuoteNumber, quote.CustomerName, quote.Premium, quote.CreatedUtc);
        return CreatedAtAction(nameof(Get), response);
    }
}
