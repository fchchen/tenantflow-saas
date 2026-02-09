using Microsoft.EntityFrameworkCore;
using TenantFlow.Data.Abstractions;
using TenantFlow.Data.Entities;

namespace TenantFlow.Data;

public class TenantFlowDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public TenantFlowDbContext(DbContextOptions<TenantFlowDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();
    public DbSet<UsageEvent> UsageEvents => Set<UsageEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Quote> Quotes => Set<Quote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>().HasIndex(t => t.Slug).IsUnique();
        modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<TenantUser>()
            .HasKey(tu => new { tu.TenantId, tu.UserId });

        modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.Tenant)
            .WithMany(t => t.TenantUsers)
            .HasForeignKey(tu => tu.TenantId);

        modelBuilder.Entity<TenantUser>()
            .HasOne(tu => tu.User)
            .WithMany(u => u.TenantUsers)
            .HasForeignKey(tu => tu.UserId);

        modelBuilder.Entity<FeatureFlag>()
            .HasIndex(ff => new { ff.TenantId, ff.Key })
            .IsUnique();

        modelBuilder.Entity<Quote>()
            .HasIndex(q => new { q.TenantId, q.QuoteNumber })
            .IsUnique();

        modelBuilder.Entity<Quote>()
            .HasQueryFilter(q => q.TenantId == _tenantContext.TenantId && q.DeletedUtc == null);

        modelBuilder.Entity<FeatureFlag>()
            .HasQueryFilter(ff => ff.TenantId == _tenantContext.TenantId && ff.DeletedUtc == null);

        modelBuilder.Entity<AppUser>()
            .HasQueryFilter(u => u.DeletedUtc == null);
    }
}
