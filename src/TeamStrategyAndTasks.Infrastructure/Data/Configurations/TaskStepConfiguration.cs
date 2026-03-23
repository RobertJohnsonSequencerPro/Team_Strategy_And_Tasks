using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class TaskStepConfiguration : IEntityTypeConfiguration<TaskStep>
{
    public void Configure(EntityTypeBuilder<TaskStep> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(s => s.Title).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");

        builder.HasOne(s => s.WorkTask)
            .WithMany(t => t.Steps)
            .HasForeignKey(s => s.WorkTaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.WorkTaskId);
        builder.HasIndex(s => s.AssigneeId);
    }
}
