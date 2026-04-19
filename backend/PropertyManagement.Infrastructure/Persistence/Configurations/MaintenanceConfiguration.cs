using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Maintenance;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class MaintenanceConfiguration : IEntityTypeConfiguration<MaintenanceRequest>
{
    public void Configure(EntityTypeBuilder<MaintenanceRequest> b)
    {
        b.ToTable("maintenance_requests");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(r => r.LeaseId).HasColumnName("lease_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(r => r.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
        b.Property(r => r.Description).HasColumnName("description").HasColumnType("text").IsRequired();
        b.Property(r => r.Priority).HasColumnName("priority").HasConversion<string>().IsRequired();
        b.Property(r => r.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(r => r.AssignedTo).HasColumnName("assigned_to").HasMaxLength(255);
        b.Property(r => r.ReportedAt).HasColumnName("reported_at");
        b.Property(r => r.ResolvedAt).HasColumnName("resolved_at");
        b.Ignore("DomainEvents");
    }
}
