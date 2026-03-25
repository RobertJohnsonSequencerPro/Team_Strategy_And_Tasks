using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ControlPlanRevisionConfiguration : IEntityTypeConfiguration<ControlPlanRevision>
{
    public void Configure(EntityTypeBuilder<ControlPlanRevision> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RevisionLabel)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(r => r.ToStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Comments)
            .HasMaxLength(2000);

        builder.HasIndex(r => r.ControlPlanId);
        builder.HasIndex(r => new { r.ControlPlanId, r.ChangedAt });
    }
}
