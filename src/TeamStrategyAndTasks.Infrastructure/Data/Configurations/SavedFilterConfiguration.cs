using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class SavedFilterConfiguration : IEntityTypeConfiguration<SavedFilter>
{
    public void Configure(EntityTypeBuilder<SavedFilter> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(f => f.Name).HasMaxLength(100).IsRequired();
        builder.Property(f => f.PageKey).HasMaxLength(50).IsRequired();
        builder.Property(f => f.FilterJson).HasColumnType("text").IsRequired();

        builder.HasIndex(f => new { f.UserId, f.PageKey });
    }
}
