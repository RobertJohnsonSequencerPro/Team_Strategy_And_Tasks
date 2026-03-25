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

    // ── M:M join entities ───────────────────────────────────────────────────
    public DbSet<ObjectiveProcess> ObjectiveProcesses => Set<ObjectiveProcess>();
    public DbSet<ProcessInitiative> ProcessInitiatives => Set<ProcessInitiative>();
    public DbSet<InitiativeWorkTask> InitiativeWorkTasks => Set<InitiativeWorkTask>();

    // ── Supporting ──────────────────────────────────────────────────────────
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SavedFilter> SavedFilters => Set<SavedFilter>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<NodeDependency> NodeDependencies => Set<NodeDependency>();
    public DbSet<KeyResult> KeyResults => Set<KeyResult>();
    public DbSet<Milestone> Milestones => Set<Milestone>();
    public DbSet<NodeRisk> NodeRisks => Set<NodeRisk>();
    public DbSet<Decision> Decisions => Set<Decision>();
    public DbSet<DecisionNodeLink> DecisionNodeLinks => Set<DecisionNodeLink>();
    public DbSet<SharedValue> SharedValues => Set<SharedValue>();

    // ── Quality Engineering ──────────────────────────────────────────────────
    public DbSet<QualityClause> QualityClauses => Set<QualityClause>();
    public DbSet<ClauseEvidenceItem> ClauseEvidenceItems => Set<ClauseEvidenceItem>();
    public DbSet<ClauseReviewEvent> ClauseReviewEvents => Set<ClauseReviewEvent>();

    // ── Quality Engineering — PFMEA ──────────────────────────────────────────
    public DbSet<PfmeaRecord> PfmeaRecords => Set<PfmeaRecord>();
    public DbSet<PfmeaFailureMode> PfmeaFailureModes => Set<PfmeaFailureMode>();
    public DbSet<PfmeaAction> PfmeaActions => Set<PfmeaAction>();

    // ── Quality Engineering — Control Plans ──────────────────────────────────
    public DbSet<ControlPlan> ControlPlans => Set<ControlPlan>();
    public DbSet<ControlPlanCharacteristic> ControlPlanCharacteristics => Set<ControlPlanCharacteristic>();
    public DbSet<ControlPlanRevision> ControlPlanRevisions => Set<ControlPlanRevision>();

    // ── Quality Engineering — Audits & Findings ───────────────────────────────
    public DbSet<Audit> Audits => Set<Audit>();
    public DbSet<AuditFinding> AuditFindings => Set<AuditFinding>();

    // ── Quality Engineering — CAPA ────────────────────────────────────────────
    public DbSet<CapaCase> CapaCases => Set<CapaCase>();
    public DbSet<CapaAction> CapaActions => Set<CapaAction>();
    public DbSet<EffectivenessCheck> EffectivenessChecks => Set<EffectivenessCheck>();

    // ── Quality Engineering — RCA Library ────────────────────────────────────
    public DbSet<RcaCase> RcaCases => Set<RcaCase>();
    public DbSet<RcaCaseTag> RcaCaseTags => Set<RcaCaseTag>();
    public DbSet<FiveWhyNode> FiveWhyNodes => Set<FiveWhyNode>();
    public DbSet<IshikawaCause> IshikawaCauses => Set<IshikawaCause>();

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
