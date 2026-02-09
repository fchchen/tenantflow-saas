# Azure Free Tier Deployment Notes

Recommended target for portfolio use:

- Frontend: Azure Static Web Apps Free
- API: Azure App Service F1
- Data: SQLite for demo or migrate to Azure SQL when budget allows

## App Settings

API requires:
- Jwt__Key
- Jwt__Issuer
- Jwt__Audience

CORS origin should include the deployed frontend URL.
