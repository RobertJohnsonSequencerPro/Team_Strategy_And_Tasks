using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Entities.Suggestions;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Live hierarchy ──────────────────────────────────────────────────────
    public DbSet<Objective> Objectives => Set<Objective>();
    public DbSet<BusinessProcess> BusinessProcesses => Set<BusinessProcess>();
    public DbSet<Initiative> Initiatives => Set<Initiative>();
    public DbSet<WorkTask> WorkTasks => Set<WorkTask>();
    public DbSet<TaskStep> TaskSteps => Set<TaskStep>();

    // ── M:M join entities ───────────────────────────────────────────────────
    public DbSet<ObjectiveProcess> ObjectiveProcesses => Set<ObjectiveProcess>();
    public DbSet<ProcessInitiative> ProcessInitiatives => Set<ProcessInitiative>();
    public DbSet<InitiativeWorkTask> InitiativeWorkTasks => Set<InitiativeWorkTask>();

    // ── Supporting ──────────────────────────────────────────────────────────
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ── Suggestion library (read-only seed data) ────────────────────────────
    public DbSet<SuggestionObjective> SuggestionObjectives => Set<SuggestionObjective>();
    public DbSet<SuggestionProcess> SuggestionProcesses => Set<SuggestionProcess>();
    public DbSet<SuggestionInitiative> SuggestionInitiatives => Set<SuggestionInitiative>();
    public DbSet<SuggestionTask> SuggestionTasks => Set<SuggestionTask>();
    public DbSet<SuggestionObjectiveProcess> SuggestionObjectiveProcesses => Set<SuggestionObjectiveProcess>();
    public DbSet<SuggestionProcessInitiative> SuggestionProcessInitiatives => Set<SuggestionProcessInitiative>();
    public DbSet<SuggestionInitiativeTask> SuggestionInitiativeTasks => Set<SuggestionInitiativeTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Picks up all IEntityTypeConfiguration<T> implementations in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    // Automatically update UpdatedAt on every SaveChanges call
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampUpdatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken ct = default)
    {
        StampUpdatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, ct);
    }

    private void StampUpdatedAt()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified
                        && e.Entity is BaseEntity);
        foreach (var entry in entries)
            ((BaseEntity)entry.Entity).UpdatedAt = DateTimeOffset.UtcNow;
    }
}
