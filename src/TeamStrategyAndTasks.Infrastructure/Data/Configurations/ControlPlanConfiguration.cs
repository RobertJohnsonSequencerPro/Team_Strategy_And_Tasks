using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ControlPlanConfiguration : IEntityTypeConfiguration<ControlPlan>
{
    public void Configure(EntityTypeBuilder<ControlPlan> builder)
    {
        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.ProcessItem)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.PartNumber)
            .HasMaxLength(100);

        builder.Property(cp => cp.PartDescription)
            .HasMaxLength(500);

        builder.Property(cp => cp.Revision)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cp => cp.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasMany(cp => cp.Characteristics)
            .WithOne(c => c.ControlPlan)
            .HasForeignKey(c => c.ControlPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(cp => cp.Revisions)
            .WithOne(r => r.ControlPlan)
            .HasForeignKey(r => r.ControlPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(cp => new { cp.IsArchived, cp.CreatedAt });
        builder.HasIndex(cp => cp.Status);
    }
}
