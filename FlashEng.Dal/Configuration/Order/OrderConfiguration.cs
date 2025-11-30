using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashEng.Dal.Configuration.Order
{
    public class OrderConfiguration : IEntityTypeConfiguration<Domain.models.Order>
    {
        public void Configure(EntityTypeBuilder<Domain.models.Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.OrderId);

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(o => o.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            builder.Property(o => o.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationship: Many Orders -> One User
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
