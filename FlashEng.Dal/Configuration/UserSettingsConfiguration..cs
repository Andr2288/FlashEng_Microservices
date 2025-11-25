using FlashEng.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashEng.Dal.Configuration
{
    public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
    {
        public void Configure(EntityTypeBuilder<UserSettings> builder)
        {
            builder.ToTable("UserSettings");

            builder.HasKey(us => us.SettingsId);

            builder.Property(us => us.Theme)
                .HasMaxLength(20)
                .HasDefaultValue("Light");

            builder.Property(us => us.Language)
                .HasMaxLength(5)
                .HasDefaultValue("en");

            builder.Property(us => us.NotificationsEnabled)
                .HasDefaultValue(true);

            // Relationship: One User -> One UserSettings
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(us => us.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserSettings_UserId");
        }
    }
}
