using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class FiveWhyNodeConfiguration : IEntityTypeConfiguration<FiveWhyNode>
{
    public void Configure(EntityTypeBuilder<FiveWhyNode> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.WhyQuestion).IsRequired().HasMaxLength(500);
        builder.Property(n => n.BecauseAnswer).HasMaxLength(1000);

        // Self-referencing parent–child relationship.
        // Restrict so callers must re-parent children before deleting a node.
        // Top-level cascade from RcaCase handles deletion of the full tree.
        builder.HasOne(n => n.Parent)
            .WithMany(n => n.Children)
            .HasForeignKey(n => n.ParentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.RcaCaseId);
        builder.HasIndex(n => n.ParentId);
    }
}
