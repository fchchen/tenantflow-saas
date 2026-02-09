# ADR-002: Tenant Resolution Through JWT

Status: Accepted

## Context

Every API request needs tenant and role context for authorization and data scoping.

## Decision

Resolve TenantId and roles from JWT claims in middleware, then expose the values through request-scoped ITenantContext.

## Consequences

Positive:
- consistent tenant resolution in one place
- clear authorization and auditing context
- testable isolation behavior

Negative:
- requires careful token issuance and claim validation
- platform impersonation support must be constrained

## Follow-up

Add external identity provider integration while preserving the same tenant context abstraction.
