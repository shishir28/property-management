using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Tenants;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(t => t.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        b.Property(t => t.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        b.Property(t => t.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        b.Property(t => t.Phone).HasColumnName("phone").HasMaxLength(20);
        b.Property(t => t.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(t => t.CreatedAt).HasColumnName("created_at");
        b.Ignore(t => t.FullName);
        b.Ignore("DomainEvents");
    }
}
