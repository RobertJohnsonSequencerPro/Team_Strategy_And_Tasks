using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ObjectiveConfiguration : IEntityTypeConfiguration<Objective>
{
    public void Configure(EntityTypeBuilder<Objective> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.Title).HasMaxLength(256).IsRequired();
        builder.Property(o => o.Description).HasColumnType("text");
        builder.Property(o => o.SuccessMetric).HasMaxLength(512);
        builder.Property(o => o.TargetValue).HasMaxLength(256);
        builder.HasIndex(o => o.OwnerId);
        builder.HasIndex(o => o.IsArchived);
        builder.HasOne(o => o.Team)
            .WithMany()
            .HasForeignKey(o => o.TeamId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
