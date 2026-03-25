using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class PfmeaRecordConfiguration : IEntityTypeConfiguration<PfmeaRecord>
{
    public void Configure(EntityTypeBuilder<PfmeaRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.ProcessItem)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(r => r.Scope)
            .HasMaxLength(1000);

        builder.Property(r => r.Revision)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasMany(r => r.FailureModes)
            .WithOne(fm => fm.Pfmea)
            .HasForeignKey(fm => fm.PfmeaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.IsArchived, r.CreatedAt });
    }
}
