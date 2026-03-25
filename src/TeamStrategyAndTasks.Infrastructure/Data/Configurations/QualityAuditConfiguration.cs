using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class QualityAuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Scope)
            .HasMaxLength(1000);

        builder.Property(a => a.Notes)
            .HasMaxLength(2000);

        builder.Property(a => a.AuditType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasMany(a => a.Findings)
            .WithOne(f => f.Audit)
            .HasForeignKey(f => f.AuditId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.IsArchived, a.CreatedAt });
        builder.HasIndex(a => a.Status);
    }
}
