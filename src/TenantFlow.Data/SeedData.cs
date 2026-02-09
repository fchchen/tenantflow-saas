using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TenantFlow.Data.Entities;

namespace TenantFlow.Data;

public static class SeedData
{
    public const string DemoPassword = "Pass123$";

    public static async Task EnsureSeededAsync(TenantFlowDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (await db.Tenants.AnyAsync())
        {
            return;
        }

        var tenants = new[]
        {
            new Tenant { Id = DemoIds.PlatformTenantId, Name = "TenantFlow Platform", Slug = "platform", IsActive = true },
            new Tenant { Id = DemoIds.AcmeTenantId, Name = "Acme Logistics", Slug = "acme", IsActive = true },
            new Tenant { Id = DemoIds.GlobexTenantId, Name = "Globex Manufacturing", Slug = "globex", IsActive = true }
        };

        var users = new[]
        {
            new AppUser { Id = DemoIds.PlatformAdminUserId, Email = "platform@tenantflow.dev", DisplayName = "Platform Admin" },
            new AppUser { Id = DemoIds.AcmeAdminUserId, Email = "admin@acme.dev", DisplayName = "Acme Admin" },
            new AppUser { Id = DemoIds.AcmeUserUserId, Email = "user@acme.dev", DisplayName = "Acme User" },
            new AppUser { Id = DemoIds.GlobexAdminUserId, Email = "admin@globex.dev", DisplayName = "Globex Admin" }
        };

        var hasher = new PasswordHasher<AppUser>();
        foreach (var user in users)
        {
            user.PasswordHash = hasher.HashPassword(user, DemoPassword);
        }

        var memberships = new[]
        {
            new TenantUser { TenantId = DemoIds.PlatformTenantId, UserId = DemoIds.PlatformAdminUserId, Role = "PlatformAdmin" },
            new TenantUser { TenantId = DemoIds.AcmeTenantId, UserId = DemoIds.AcmeAdminUserId, Role = "TenantAdmin" },
            new TenantUser { TenantId = DemoIds.AcmeTenantId, UserId = DemoIds.AcmeUserUserId, Role = "TenantUser" },
            new TenantUser { TenantId = DemoIds.GlobexTenantId, UserId = DemoIds.GlobexAdminUserId, Role = "TenantAdmin" }
        };

        var flags = new[]
        {
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = DemoIds.AcmeTenantId, Key = "quote.create", IsEnabled = true, RolloutPercent = 100 },
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = DemoIds.AcmeTenantId, Key = "tenant.user.invite", IsEnabled = true, RolloutPercent = 100 },
            new FeatureFlag { Id = Guid.NewGuid(), TenantId = DemoIds.GlobexTenantId, Key = "quote.create", IsEnabled = true, RolloutPercent = 100 }
        };

        db.Tenants.AddRange(tenants);
        db.Users.AddRange(users);
        db.TenantUsers.AddRange(memberships);
        db.FeatureFlags.AddRange(flags);

        await db.SaveChangesAsync();
    }
}
