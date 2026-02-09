# Usage Metering

TenantFlow records usage events for tenant activity.

## Current Events

- quote.created with quantity 1 for each quote creation.

## Storage

Usage events are stored in UsageEvents with:
- TenantId
- EventType
- Quantity
- MetadataJson
- OccurredUtc

## Reporting

Platform admin usage endpoint aggregates total quantity per tenant.
