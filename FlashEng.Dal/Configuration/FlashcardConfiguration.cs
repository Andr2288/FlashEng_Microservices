using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FlashEng.Domain.models;

namespace FlashEng.Dal.Configuration
{
    public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
    {
        public void Configure(EntityTypeBuilder<Flashcard> builder)
        {
            builder.ToTable("Flashcards");

            builder.HasKey(f => f.FlashcardId);

            builder.Property(f => f.Category)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(f => f.EnglishWord)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Translation)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.Definition)
                .HasColumnType("TEXT");

            builder.Property(f => f.ExampleSentence)
                .HasColumnType("TEXT");

            builder.Property(f => f.Pronunciation)
                .HasMaxLength(100);

            builder.Property(f => f.AudioUrl)
                .HasMaxLength(500);

            builder.Property(f => f.ImageUrl)
                .HasMaxLength(500);

            builder.Property(f => f.Difficulty)
                .HasMaxLength(20)
                .HasDefaultValue("Medium");

            builder.Property(f => f.IsPublic)
                .HasDefaultValue(false);

            builder.Property(f => f.Price)
                .HasColumnType("decimal(10,2)");

            builder.Property(f => f.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(f => f.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .ValueGeneratedOnAddOrUpdate();

            // Indexes
            builder.HasIndex(f => f.UserId)
                .HasDatabaseName("IX_Flashcards_UserId");

            builder.HasIndex(f => f.Category)
                .HasDatabaseName("IX_Flashcards_Category");

            builder.HasIndex(f => f.EnglishWord)
                .HasDatabaseName("IX_Flashcards_EnglishWord");

            // Relationship: Many Flashcards -> One User
            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
