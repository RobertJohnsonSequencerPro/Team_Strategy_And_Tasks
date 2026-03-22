using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ObjectiveProcessConfiguration : IEntityTypeConfiguration<ObjectiveProcess>
{
    public void Configure(EntityTypeBuilder<ObjectiveProcess> builder)
    {
        builder.HasKey(op => new { op.ObjectiveId, op.ProcessId });

        builder.HasOne(op => op.Objective)
               .WithMany(o => o.ObjectiveProcesses)
               .HasForeignKey(op => op.ObjectiveId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(op => op.Process)
               .WithMany(p => p.ObjectiveProcesses)
               .HasForeignKey(op => op.ProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(op => op.LinkedAt).HasDefaultValueSql("now()");
    }
}
