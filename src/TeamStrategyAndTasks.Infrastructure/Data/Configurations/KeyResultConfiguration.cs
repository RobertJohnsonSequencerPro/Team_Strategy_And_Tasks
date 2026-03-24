using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class KeyResultConfiguration : IEntityTypeConfiguration<KeyResult>
{
    public void Configure(EntityTypeBuilder<KeyResult> builder)
    {
        builder.HasKey(k => k.Id);

        builder.Property(k => k.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(k => k.Unit)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(k => k.CurrentValue)
               .HasPrecision(18, 4);

        builder.Property(k => k.TargetValue)
               .HasPrecision(18, 4);

        builder.HasOne(k => k.Objective)
               .WithMany(o => o.KeyResults)
               .HasForeignKey(k => k.ObjectiveId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(k => k.ObjectiveId);
    }
}
