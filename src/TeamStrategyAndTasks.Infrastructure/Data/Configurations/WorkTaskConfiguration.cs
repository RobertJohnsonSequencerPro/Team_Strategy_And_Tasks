using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.Title).HasMaxLength(256).IsRequired();
        builder.Property(t => t.Description).HasColumnType("text");
        builder.Property(t => t.EstimatedEffort).HasPrecision(10, 2);
        builder.Property(t => t.ActualEffort).HasPrecision(10, 2);
        builder.HasIndex(t => t.OwnerId);
        builder.HasIndex(t => t.AssigneeId);
        builder.HasIndex(t => t.IsArchived);
    }
}
