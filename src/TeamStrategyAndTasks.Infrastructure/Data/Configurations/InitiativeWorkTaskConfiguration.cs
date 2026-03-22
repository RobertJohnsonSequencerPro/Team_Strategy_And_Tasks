using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class InitiativeWorkTaskConfiguration : IEntityTypeConfiguration<InitiativeWorkTask>
{
    public void Configure(EntityTypeBuilder<InitiativeWorkTask> builder)
    {
        builder.HasKey(it => new { it.InitiativeId, it.WorkTaskId });

        builder.HasOne(it => it.Initiative)
               .WithMany(i => i.InitiativeWorkTasks)
               .HasForeignKey(it => it.InitiativeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(it => it.WorkTask)
               .WithMany(t => t.InitiativeWorkTasks)
               .HasForeignKey(it => it.WorkTaskId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(it => it.LinkedAt).HasDefaultValueSql("now()");
    }
}
