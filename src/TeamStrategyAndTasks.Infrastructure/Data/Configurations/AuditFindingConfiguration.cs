using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class AuditFindingConfiguration : IEntityTypeConfiguration<AuditFinding>
{
    public void Configure(EntityTypeBuilder<AuditFinding> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Description)
            .HasMaxLength(2000);

        builder.Property(f => f.ClauseReference)
            .HasMaxLength(50);

        builder.Property(f => f.FindingType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(f => f.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(f => f.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(40);

        builder.Property(f => f.ContainmentNotes)
            .HasMaxLength(2000);

        builder.Property(f => f.RootCauseNotes)
            .HasMaxLength(2000);

        builder.Property(f => f.CorrectiveActionNotes)
            .HasMaxLength(2000);

        builder.Property(f => f.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(f => f.AuditId);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => new { f.AuditId, f.Status });
    }
}
