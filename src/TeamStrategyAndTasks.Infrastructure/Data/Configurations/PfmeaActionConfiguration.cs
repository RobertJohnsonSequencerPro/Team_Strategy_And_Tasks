using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class PfmeaActionConfiguration : IEntityTypeConfiguration<PfmeaAction>
{
    public void Configure(EntityTypeBuilder<PfmeaAction> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.OutcomeNotes)
            .HasMaxLength(2000);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(a => a.FailureModeId);
        builder.HasIndex(a => new { a.Status, a.TargetDate });
    }
}
