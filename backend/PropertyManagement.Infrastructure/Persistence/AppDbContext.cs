using Microsoft.EntityFrameworkCore;
using PropertyManagement.Domain.Common;
using PropertyManagement.Domain.Finance;
using PropertyManagement.Domain.Inspections;
using PropertyManagement.Domain.Leases;
using PropertyManagement.Domain.Maintenance;
using PropertyManagement.Domain.Properties;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasCharSet("utf8mb4");
        builder.UseCollation("utf8mb4_bin");
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // dispatch domain events before persisting
        var events = ChangeTracker.Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<Entity>())
            entry.Entity.ClearDomainEvents();

        var result = await base.SaveChangesAsync(ct);
        return result;
    }
}
