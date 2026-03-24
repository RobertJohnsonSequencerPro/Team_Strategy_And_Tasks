using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class NodeDependencyConfiguration : IEntityTypeConfiguration<NodeDependency>
{
    public void Configure(EntityTypeBuilder<NodeDependency> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.BlockerType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(d => d.BlockedType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(d => d.DependencyType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(d => d.Notes).HasMaxLength(500);

        // Quick look-up of all prerequisites for a given node
        builder.HasIndex(d => new { d.BlockedType,  d.BlockedId  });
        // Quick look-up of all downstream dependents of a given node
        builder.HasIndex(d => new { d.BlockerType, d.BlockerId });

        // No duplicate edges
        builder.HasIndex(d => new { d.BlockerType, d.BlockerId, d.BlockedType, d.BlockedId })
               .IsUnique();
    }
}
