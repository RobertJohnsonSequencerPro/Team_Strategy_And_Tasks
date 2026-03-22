using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).UseIdentityColumn();
        builder.Property(a => a.OccurredAt).HasDefaultValueSql("now()");
        builder.Property(a => a.FieldName).HasMaxLength(128).IsRequired();
        builder.Property(a => a.OldValue).HasColumnType("text");
        builder.Property(a => a.NewValue).HasColumnType("text");
        builder.HasIndex(a => new { a.NodeType, a.NodeId });
        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.OccurredAt);
    }
}
