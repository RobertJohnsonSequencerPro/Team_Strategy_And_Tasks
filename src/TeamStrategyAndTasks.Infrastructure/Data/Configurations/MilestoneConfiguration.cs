using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(m => m.Notes)
               .HasMaxLength(1000);

        builder.Property(m => m.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.HasOne(m => m.Initiative)
               .WithMany(i => i.Milestones)
               .HasForeignKey(m => m.InitiativeId)
               .OnDelete(DeleteBehavior.Cascade);

        // Primary look-up: all milestones for an initiative ordered by due date
        builder.HasIndex(m => new { m.InitiativeId, m.DueDate });

        // Nightly job query: all pending milestones past due date
        builder.HasIndex(m => new { m.Status, m.DueDate });
    }
}
