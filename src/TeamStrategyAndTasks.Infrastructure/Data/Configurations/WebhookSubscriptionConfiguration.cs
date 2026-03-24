using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Description).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Url).HasMaxLength(500).IsRequired();
        builder.Property(w => w.EventFilter).HasMaxLength(200).HasDefaultValue("*");
        builder.Property(w => w.Secret).HasMaxLength(256);
    }
}
