using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(n => n.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(n => n.Message).HasColumnType("text").IsRequired();
        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}
