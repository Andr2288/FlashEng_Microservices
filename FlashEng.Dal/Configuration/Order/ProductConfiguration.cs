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
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(p => p.Price)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(p => p.IsAvailable)
                .HasDefaultValue(true);

            builder.Property(p => p.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
