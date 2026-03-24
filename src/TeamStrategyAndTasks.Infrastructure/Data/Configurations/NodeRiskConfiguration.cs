using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class NodeRiskConfiguration : IEntityTypeConfiguration<NodeRisk>
{
    public void Configure(EntityTypeBuilder<NodeRisk> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.NodeType)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        builder.Property(r => r.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(r => r.Description).HasMaxLength(2000);
        builder.Property(r => r.MitigationPlan).HasMaxLength(2000);

        builder.Property(r => r.Probability)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(10);

        builder.Property(r => r.Impact)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(10);

        builder.Property(r => r.Status)
               .IsRequired()
               .HasConversion<string>()
               .HasMaxLength(20);

        // Severity is computed in C# — ignore as a DB column
        builder.Ignore(r => r.Severity);

        // Primary look-up: all risks for a node
        builder.HasIndex(r => new { r.NodeType, r.NodeId });

        // Global dashboard filter: open risks ordered by severity fields
        builder.HasIndex(r => new { r.Status, r.Probability, r.Impact });
    }
}
