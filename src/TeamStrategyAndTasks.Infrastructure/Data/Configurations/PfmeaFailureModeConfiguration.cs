using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class PfmeaFailureModeConfiguration : IEntityTypeConfiguration<PfmeaFailureMode>
{
    public void Configure(EntityTypeBuilder<PfmeaFailureMode> builder)
    {
        builder.HasKey(fm => fm.Id);

        builder.Property(fm => fm.ProcessStep)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(fm => fm.ProcessFunction)
            .HasMaxLength(500);

        builder.Property(fm => fm.FailureDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(fm => fm.PotentialEffect)
            .HasMaxLength(500);

        builder.Property(fm => fm.PotentialCause)
            .HasMaxLength(500);

        builder.Property(fm => fm.CurrentControls)
            .HasMaxLength(1000);

        builder.Property(fm => fm.Notes)
            .HasMaxLength(2000);

        builder.HasMany(fm => fm.Actions)
            .WithOne(a => a.FailureMode)
            .HasForeignKey(a => a.FailureModeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fm => fm.PfmeaId);
        builder.HasIndex(fm => new { fm.PfmeaId, fm.SortOrder });
    }
}
