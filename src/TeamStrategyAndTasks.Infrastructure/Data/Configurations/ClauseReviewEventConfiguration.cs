using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ClauseReviewEventConfiguration : IEntityTypeConfiguration<ClauseReviewEvent>
{
    public void Configure(EntityTypeBuilder<ClauseReviewEvent> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.PreviousStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.NewStatus)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(r => r.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(r => r.ClauseId);
        builder.HasIndex(r => r.ReviewedAt);
    }
}
