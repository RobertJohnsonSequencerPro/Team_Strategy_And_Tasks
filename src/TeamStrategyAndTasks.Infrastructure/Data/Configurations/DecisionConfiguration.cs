using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class DecisionConfiguration : IEntityTypeConfiguration<Decision>
{
    public void Configure(EntityTypeBuilder<Decision> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
               .IsRequired()
               .HasMaxLength(300);

        builder.Property(d => d.Context).HasMaxLength(4000);
        builder.Property(d => d.Rationale).HasMaxLength(4000);
        builder.Property(d => d.AlternativesConsidered).HasMaxLength(4000);

        builder.Property(d => d.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        // Self-referencing supersession chain — restrict delete so that superseding
        // a decision does not cascade-delete the older one.
        builder.HasOne(d => d.SupersededBy)
               .WithMany()
               .HasForeignKey(d => d.SupersededById)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.NodeLinks)
               .WithOne(nl => nl.Decision)
               .HasForeignKey(nl => nl.DecisionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.MadeAt);
        builder.HasIndex(d => d.Status);
    }
}

public class DecisionNodeLinkConfiguration : IEntityTypeConfiguration<DecisionNodeLink>
{
    public void Configure(EntityTypeBuilder<DecisionNodeLink> builder)
    {
        // Composite primary key
        builder.HasKey(nl => new { nl.DecisionId, nl.NodeType, nl.NodeId });

        builder.Property(nl => nl.NodeType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        // Index for looking up all decisions attached to a node efficiently
        builder.HasIndex(nl => new { nl.NodeType, nl.NodeId });
    }
}
