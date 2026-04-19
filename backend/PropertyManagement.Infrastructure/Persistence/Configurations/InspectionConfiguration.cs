using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Inspections;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> b)
    {
        b.ToTable("inspections");
        b.HasKey(i => i.Id);
        b.Property(i => i.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(i => i.PropertyId).HasColumnName("property_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(i => i.LeaseId).HasColumnName("lease_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(i => i.Type).HasColumnName("type").HasConversion<string>().IsRequired();
        b.Property(i => i.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(i => i.ScheduledAt).HasColumnName("scheduled_at");
        b.Property(i => i.CompletedAt).HasColumnName("completed_at");
        b.Property(i => i.Notes).HasColumnName("notes").HasColumnType("text");
        b.Property(i => i.CreatedAt).HasColumnName("created_at");
        b.Ignore("DomainEvents");
    }
}
