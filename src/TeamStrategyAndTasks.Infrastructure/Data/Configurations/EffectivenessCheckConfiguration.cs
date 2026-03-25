using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class EffectivenessCheckConfiguration : IEntityTypeConfiguration<EffectivenessCheck>
{
    public void Configure(EntityTypeBuilder<EffectivenessCheck> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Verdict)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.CapaCaseId);
        builder.HasIndex(e => new { e.CapaCaseId, e.CheckDate });
    }
}
