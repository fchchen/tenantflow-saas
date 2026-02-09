# Architecture Overview

TenantFlow uses a monorepo with a clean separation between API, data, tests, and UI.

## API Layer

TenantFlow.Api contains:
- authentication and JWT issuance
- tenant context middleware
- platform admin endpoints
- tenant management endpoints
- quote endpoints with feature flag checks

## Data Layer

TenantFlow.Data contains:
- EF Core DbContext
- tenant and user membership entities
- feature flag, usage event, and audit entities
- query filters for tenant isolation and soft-delete behavior

## UI Layer

client/tenantflow-ui contains:
- login and persona demo access
- platform admin dashboard
- tenant dashboard
- role and auth guards
- bearer token interceptor

## Security Boundaries

- JWT contains tenant_id and role claims
- middleware maps claims into request-scoped tenant context
- policy authorization protects platform admin routes
- tenant-scoped queries prevent cross-tenant reads

## Cost Profile

Current implementation is optimized for free-tier demonstration:
- SQLite local data store
- simple in-process caching for feature flags
- no required paid cloud dependencies
