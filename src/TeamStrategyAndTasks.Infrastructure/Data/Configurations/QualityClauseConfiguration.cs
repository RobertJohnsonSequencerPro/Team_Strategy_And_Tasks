using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class QualityClauseConfiguration : IEntityTypeConfiguration<QualityClause>
{
    public void Configure(EntityTypeBuilder<QualityClause> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClauseNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.ConformanceStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(c => c.AssessmentNotes)
            .HasMaxLength(4000);

        builder.HasMany(c => c.EvidenceItems)
            .WithOne(e => e.Clause)
            .HasForeignKey(e => e.ClauseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.ReviewHistory)
            .WithOne(r => r.Clause)
            .HasForeignKey(r => r.ClauseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ClauseNumber).IsUnique();
        builder.HasIndex(c => new { c.IsActive, c.SortOrder });
    }
}
