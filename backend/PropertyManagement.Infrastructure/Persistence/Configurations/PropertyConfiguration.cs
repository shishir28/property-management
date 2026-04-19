using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Properties;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> b)
    {
        b.ToTable("properties");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(p => p.Address).HasColumnName("address").HasMaxLength(255).IsRequired();
        b.Property(p => p.City).HasColumnName("city").HasMaxLength(100).IsRequired();
        b.Property(p => p.State).HasColumnName("state").HasMaxLength(2).IsRequired();
        b.Property(p => p.Zip).HasColumnName("zip").HasMaxLength(10).IsRequired();
        b.Property(p => p.UnitCount).HasColumnName("unit_count").IsRequired();
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Ignore("DomainEvents");
    }
}
