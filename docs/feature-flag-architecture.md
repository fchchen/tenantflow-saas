# Feature Flag Architecture

TenantFlow stores feature flags per tenant in the FeatureFlags table.

## Flow

1. API receives request.
2. Tenant context resolves current TenantId.
3. FeatureFlagService checks cache first.
4. Service falls back to database when cache misses.
5. Endpoint allows or denies action based on flag state.

## Current Flag Usage

- quote.create controls quote creation endpoint.

## Notes

- PlatformAdmin can upsert tenant flags through admin endpoints.
- Cache is short-lived in-memory and safe for demo scale.
