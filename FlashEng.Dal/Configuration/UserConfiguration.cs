using FlashEng.Domain.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashEng.Dal.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("UserProfiles");

            builder.HasKey(u => u.UserId);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Role)
                .HasMaxLength(20)
                .HasDefaultValue("User");

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("IX_UserProfiles_Email");
        }
    }
}
