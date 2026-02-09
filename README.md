# TenantFlow SaaS

TenantFlow is an enterprise-style multi-tenant SaaS reference project built for interview signal with an Azure free-tier deployment target.

## What It Demonstrates

- Single database multi-tenancy with TenantId isolation
- Role-based access control with PlatformAdmin, TenantAdmin, TenantUser
- Admin portal and tenant portal in one Angular app
- Feature flags per tenant
- Usage metering and audit logging
- Integration tests that verify tenant isolation and RBAC

## Repository Structure

- src/TenantFlow.Api: ASP.NET Core API modules
- src/TenantFlow.Data: EF Core entities and DbContext
- src/TenantFlow.Tests: xUnit unit and integration tests
- client/tenantflow-ui: Angular 17 standalone app
- docs: architecture notes and ADRs

## Local Run

Prerequisites:
- .NET SDK 10+
- Node.js 20+

API:
- cd src/TenantFlow.Api
- dotnet run

UI:
- cd client/tenantflow-ui
- npm install
- npm start

Default URLs:
- API: http://localhost:5000
- UI: http://localhost:4200

## Demo Personas

Use the Demo buttons on the login page, or login manually with password Pass123 followed by a dollar-sign character.

- platform@tenantflow.dev (PlatformAdmin)
- admin@acme.dev (TenantAdmin)
- user@acme.dev (TenantUser)
- admin@globex.dev (TenantAdmin)

## Validation Commands

Backend:
- dotnet build TenantFlow.sln
- dotnet test TenantFlow.sln

Frontend:
- cd client/tenantflow-ui
- npm run build

## Key API Routes

- POST /api/v1/auth/login
- POST /api/v1/auth/demo
- GET and POST /api/v1/admin/tenants
- GET /api/v1/admin/usage
- GET and PUT /api/v1/admin/feature-flags/{tenantId}
- GET, POST, PATCH, DELETE /api/v1/tenant/users
- GET /api/v1/tenant/feature-flags
- GET and POST /api/v1/quotes

## Azure Free Tier Target

- Frontend: Azure Static Web Apps Free
- API: Azure App Service F1
- Data: SQLite for local/demo; Azure SQL as later upgrade

## Docs

- docs/architecture.md
- docs/tenancy-model.md
- docs/decisions/ADR-001-single-db-tenantid.md
- docs/decisions/ADR-002-jwt-tenant-context.md
