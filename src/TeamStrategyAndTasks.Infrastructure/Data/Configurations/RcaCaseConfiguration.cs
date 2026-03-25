using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class RcaCaseConfiguration : IEntityTypeConfiguration<RcaCase>
{
    public void Configure(EntityTypeBuilder<RcaCase> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ProblemStatement).HasMaxLength(4000);
        builder.Property(r => r.RcaType).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.ProcessArea).HasMaxLength(200);
        builder.Property(r => r.PartFamily).HasMaxLength(200);
        builder.Property(r => r.RootCauseSummary).HasMaxLength(4000);

        builder.HasMany(r => r.Tags)
            .WithOne(t => t.RcaCase)
            .HasForeignKey(t => t.RcaCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.FiveWhyNodes)
            .WithOne(n => n.RcaCase)
            .HasForeignKey(n => n.RcaCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.IshikawaCauses)
            .WithOne(c => c.RcaCase)
            .HasForeignKey(c => c.RcaCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.IsArchived, r.CreatedAt });
        builder.HasIndex(r => r.ProcessArea);
    }
}
