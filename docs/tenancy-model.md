# Tenancy Model

TenantFlow uses single-database multi-tenancy.

## Isolation Strategy

- All tenant-owned records include TenantId.
- Tenant context is resolved from JWT tenant_id claim.
- EF query filters scope tenant-owned entities to the current TenantId.
- PlatformAdmin can override tenant context by sending X-Tenant-Id for administrative workflows.

## Entities

Primary entities:
- Tenants
- Users
- TenantUsers (many-to-many with role)
- FeatureFlags
- UsageEvents
- AuditLogs
- Quotes

## Soft Delete

- User and quote-related entities implement soft delete where appropriate.
- Query filters exclude deleted records by default.

## Risks and Mitigations

Risk: accidental cross-tenant query.
Mitigation: central tenant context, query filters, and integration tests.

Risk: role escalation.
Mitigation: explicit policies and role checks for admin actions.
