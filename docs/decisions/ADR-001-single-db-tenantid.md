# ADR-001: Single Database With TenantId

Status: Accepted

## Context

The project target is strong enterprise signal with free-tier affordability.

## Decision

Use one relational database with TenantId-based partitioning at the application layer.

## Consequences

Positive:
- fastest path to a working multi-tenant prototype
- low operational cost
- easy local development

Negative:
- weaker hard isolation than database-per-tenant
- strict application discipline required

## Follow-up

If needed, evolve to schema-per-tenant or database-per-tenant for premium tiers.
