using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ControlPlanCharacteristicConfiguration : IEntityTypeConfiguration<ControlPlanCharacteristic>
{
    public void Configure(EntityTypeBuilder<ControlPlanCharacteristic> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ProcessStep)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ProcessOperation)
            .HasMaxLength(200);

        builder.Property(c => c.CharacteristicNo)
            .HasMaxLength(50);

        builder.Property(c => c.CharacteristicType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.CharacteristicDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.SpecificationTolerance)
            .HasMaxLength(200);

        builder.Property(c => c.ControlMethod)
            .HasMaxLength(500);

        builder.Property(c => c.SamplingSize)
            .HasMaxLength(100);

        builder.Property(c => c.SamplingFrequency)
            .HasMaxLength(100);

        builder.Property(c => c.MeasurementTechnique)
            .HasMaxLength(200);

        builder.Property(c => c.ReactionPlan)
            .HasMaxLength(1000);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.ControlPlanId);
        builder.HasIndex(c => new { c.ControlPlanId, c.SortOrder });
    }
}
