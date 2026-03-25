using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class CapaCaseConfiguration : IEntityTypeConfiguration<CapaCase>
{
    public void Configure(EntityTypeBuilder<CapaCase> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.CaseNumber)
            .HasMaxLength(50);

        builder.Property(c => c.CapaType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(c => c.ProblemStatement)
            .HasMaxLength(4000);

        builder.Property(c => c.ContainmentActions)
            .HasMaxLength(4000);

        builder.Property(c => c.RootCauseAnalysis)
            .HasMaxLength(4000);

        builder.Property(c => c.ProposedCorrection)
            .HasMaxLength(4000);

        builder.HasMany(c => c.Actions)
            .WithOne(a => a.CapaCase)
            .HasForeignKey(a => a.CapaCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.EffectivenessChecks)
            .WithOne(e => e.CapaCase)
            .HasForeignKey(e => e.CapaCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => new { c.IsArchived, c.CreatedAt });
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CaseNumber);
    }
}
