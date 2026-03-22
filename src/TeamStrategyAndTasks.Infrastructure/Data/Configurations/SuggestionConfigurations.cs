using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TeamStrategyAndTasks.Core.Entities.Suggestions;

namespace TeamStrategyAndTasks.Infrastructure.Data.Configurations;

public class SuggestionObjectiveConfiguration : IEntityTypeConfiguration<SuggestionObjective>
{
    public void Configure(EntityTypeBuilder<SuggestionObjective> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Title).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.HasIndex(s => s.IsActive);
    }
}

public class SuggestionProcessConfiguration : IEntityTypeConfiguration<SuggestionProcess>
{
    public void Configure(EntityTypeBuilder<SuggestionProcess> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Title).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.HasIndex(s => s.IsActive);
    }
}

public class SuggestionInitiativeConfiguration : IEntityTypeConfiguration<SuggestionInitiative>
{
    public void Configure(EntityTypeBuilder<SuggestionInitiative> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Title).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.HasIndex(s => s.IsActive);
    }
}

public class SuggestionTaskConfiguration : IEntityTypeConfiguration<SuggestionTask>
{
    public void Configure(EntityTypeBuilder<SuggestionTask> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.Title).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.HasIndex(s => s.IsActive);
    }
}

public class SuggestionObjectiveProcessConfiguration : IEntityTypeConfiguration<SuggestionObjectiveProcess>
{
    public void Configure(EntityTypeBuilder<SuggestionObjectiveProcess> builder)
    {
        builder.HasKey(s => new { s.SuggestionObjectiveId, s.SuggestionProcessId });
        builder.HasOne(s => s.SuggestionObjective)
               .WithMany(o => o.ObjectiveProcesses)
               .HasForeignKey(s => s.SuggestionObjectiveId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.SuggestionProcess)
               .WithMany(p => p.ObjectiveProcesses)
               .HasForeignKey(s => s.SuggestionProcessId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SuggestionProcessInitiativeConfiguration : IEntityTypeConfiguration<SuggestionProcessInitiative>
{
    public void Configure(EntityTypeBuilder<SuggestionProcessInitiative> builder)
    {
        builder.HasKey(s => new { s.SuggestionProcessId, s.SuggestionInitiativeId });
        builder.HasOne(s => s.SuggestionProcess)
               .WithMany(p => p.ProcessInitiatives)
               .HasForeignKey(s => s.SuggestionProcessId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.SuggestionInitiative)
               .WithMany(i => i.ProcessInitiatives)
               .HasForeignKey(s => s.SuggestionInitiativeId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SuggestionInitiativeTaskConfiguration : IEntityTypeConfiguration<SuggestionInitiativeTask>
{
    public void Configure(EntityTypeBuilder<SuggestionInitiativeTask> builder)
    {
        builder.HasKey(s => new { s.SuggestionInitiativeId, s.SuggestionTaskId });
        builder.HasOne(s => s.SuggestionInitiative)
               .WithMany(i => i.InitiativeTasks)
               .HasForeignKey(s => s.SuggestionInitiativeId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.SuggestionTask)
               .WithMany(t => t.InitiativeTasks)
               .HasForeignKey(s => s.SuggestionTaskId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
