using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Leases;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class LeaseConfiguration : IEntityTypeConfiguration<Lease>
{
    public void Configure(EntityTypeBuilder<Lease> b)
    {
        b.ToTable("leases");
        b.HasKey(l => l.Id);
        b.Property(l => l.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(l => l.PropertyId).HasColumnName("property_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(l => l.TenantId).HasColumnName("tenant_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(l => l.UnitNumber).HasColumnName("unit_number").HasMaxLength(20).IsRequired();
        b.Property(l => l.StartDate).HasColumnName("start_date");
        b.Property(l => l.EndDate).HasColumnName("end_date");
        b.Property(l => l.MonthlyRent).HasColumnName("monthly_rent").HasColumnType("decimal(10,2)");
        b.Property(l => l.SecurityDeposit).HasColumnName("security_deposit").HasColumnType("decimal(10,2)");
        b.Property(l => l.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(l => l.CreatedAt).HasColumnName("created_at");
        b.Ignore("DomainEvents");
    }
}
