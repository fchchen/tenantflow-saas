using System.ComponentModel.DataAnnotations;

namespace TenantFlow.Api.Models;

public sealed record CreateTenantRequest(
    [param: Required, MaxLength(120)] string Name,
    [param: Required, MaxLength(80), RegularExpression("^[a-z0-9-]+$")] string Slug);

public sealed record TenantResponse(Guid Id, string Name, string Slug, bool IsActive, int UserCount);
public sealed record UsageSummaryResponse(Guid TenantId, string TenantSlug, int TotalQuantity);

public sealed record UpsertFeatureFlagRequest(
    [param: Required, MaxLength(120), RegularExpression("^[a-z0-9.-]+$")] string Key,
    bool IsEnabled,
    [param: Range(0, 100)] int RolloutPercent = 100);

public sealed record FeatureFlagResponse(Guid Id, Guid TenantId, string Key, bool IsEnabled, int RolloutPercent, DateTime UpdatedUtc);
