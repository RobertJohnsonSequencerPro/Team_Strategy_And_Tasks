using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class CapaActionConfiguration : IEntityTypeConfiguration<CapaAction>
{
    public void Configure(EntityTypeBuilder<CapaAction> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(a => a.CapaCaseId);
        builder.HasIndex(a => a.Status);
    }
}
