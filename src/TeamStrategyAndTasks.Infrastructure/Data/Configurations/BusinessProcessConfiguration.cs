using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class BusinessProcessConfiguration : IEntityTypeConfiguration<BusinessProcess>
{
    public void Configure(EntityTypeBuilder<BusinessProcess> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.Title).HasMaxLength(256).IsRequired();
        builder.Property(p => p.Description).HasColumnType("text");
        builder.Property(p => p.SuccessMetric).HasMaxLength(512);
        builder.Property(p => p.TargetValue).HasMaxLength(256);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.IsArchived);
    }
}
