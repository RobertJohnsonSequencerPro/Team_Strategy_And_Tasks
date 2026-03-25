using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class IshikawaCauseConfiguration : IEntityTypeConfiguration<IshikawaCause>
{
    public void Configure(EntityTypeBuilder<IshikawaCause> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.CauseText).IsRequired().HasMaxLength(500);

        // Self-referencing primary–sub-cause relationship.
        builder.HasOne(c => c.ParentCause)
            .WithMany(c => c.SubCauses)
            .HasForeignKey(c => c.ParentCauseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.RcaCaseId);
        builder.HasIndex(c => new { c.RcaCaseId, c.Category });
    }
}
