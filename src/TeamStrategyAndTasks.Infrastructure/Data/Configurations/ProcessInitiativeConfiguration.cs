using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class ProcessInitiativeConfiguration : IEntityTypeConfiguration<ProcessInitiative>
{
    public void Configure(EntityTypeBuilder<ProcessInitiative> builder)
    {
        builder.HasKey(pi => new { pi.ProcessId, pi.InitiativeId });

        builder.HasOne(pi => pi.Process)
               .WithMany(p => p.ProcessInitiatives)
               .HasForeignKey(pi => pi.ProcessId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.Initiative)
               .WithMany(i => i.ProcessInitiatives)
               .HasForeignKey(pi => pi.InitiativeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(pi => pi.LinkedAt).HasDefaultValueSql("now()");
    }
}
