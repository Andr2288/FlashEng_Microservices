using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashEng.Dal.Configuration.Order.OrderConfiguration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("OrderItems");

            builder.HasKey(oi => oi.OrderItemId);

            builder.Property(oi => oi.Quantity)
                .IsRequired();

            builder.Property(oi => oi.UnitPrice)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(oi => oi.LineTotal)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            // Relationships з явним вказуванням foreign key
            builder.HasOne<Domain.models.Order>()
                .WithMany()
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ВИПРАВЛЕНО: явно вказуємо що Product використовує існуючий ProductId
            builder.HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
