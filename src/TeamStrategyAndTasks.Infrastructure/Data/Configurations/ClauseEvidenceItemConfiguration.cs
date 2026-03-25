using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ClauseEvidenceItemConfiguration : IEntityTypeConfiguration<ClauseEvidenceItem>
{
    public void Configure(EntityTypeBuilder<ClauseEvidenceItem> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EvidenceType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(2000);

        builder.Property(e => e.Url)
            .HasMaxLength(2000);

        builder.HasIndex(e => e.ClauseId);
    }
}
