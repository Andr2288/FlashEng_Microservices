using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Configuration.Order
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.Amount)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(p => p.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationship: One Order -> One Payment
            builder.HasOne<Domain.models.Order>()
                .WithMany()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.OrderId)
                .IsUnique()
                .HasDatabaseName("IX_Payments_OrderId");
        }
    }
}
