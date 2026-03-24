using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class SharedValueConfiguration : IEntityTypeConfiguration<SharedValue>
{
    public void Configure(EntityTypeBuilder<SharedValue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(v => v.Definition)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(v => v.DisplayOrder)
            .IsRequired();

        builder.HasIndex(v => new { v.IsArchived, v.DisplayOrder });
    }
}
