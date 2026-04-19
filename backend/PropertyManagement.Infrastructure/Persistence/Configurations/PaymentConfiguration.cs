using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropertyManagement.Domain.Finance;

namespace PropertyManagement.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.ToTable("payments");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(p => p.LeaseId).HasColumnName("lease_id").HasColumnType("char(36)").UseCollation("utf8mb4_bin");
        b.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
        b.Property(p => p.DueDate).HasColumnName("due_date");
        b.Property(p => p.PaidDate).HasColumnName("paid_date");
        b.Property(p => p.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        b.Property(p => p.PaymentMethod).HasColumnName("payment_method").HasMaxLength(50);
        b.Property(p => p.Notes).HasColumnName("notes").HasColumnType("text");
        b.Property(p => p.CreatedAt).HasColumnName("created_at");
        b.Ignore("DomainEvents");
    }
}
