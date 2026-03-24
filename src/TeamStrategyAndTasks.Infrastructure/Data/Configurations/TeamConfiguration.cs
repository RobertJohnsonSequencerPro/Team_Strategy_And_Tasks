using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(t => t.Name).HasMaxLength(256).IsRequired();
        // Mandate is mandatory — stored as TEXT so there is no length ceiling.
        builder.Property(t => t.Mandate).HasColumnType("text").IsRequired();
        builder.HasIndex(t => t.IsArchived);
        builder.HasIndex(t => t.Name);
    }
}
