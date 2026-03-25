using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class RcaCaseTagConfiguration : IEntityTypeConfiguration<RcaCaseTag>
{
    public void Configure(EntityTypeBuilder<RcaCaseTag> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Tag).IsRequired().HasMaxLength(100);

        builder.HasIndex(t => new { t.RcaCaseId, t.Tag }).IsUnique();
        builder.HasIndex(t => t.Tag); // for autocomplete queries
    }
}
