namespace TeamStrategyAndTasks.Web.Api;

// ── Auth ─────────────────────────────────────────────────────────────────────

public record TokenRequest(string Email, string Password);

public record TokenResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string DisplayName,
    string Email);

// ── Objective ────────────────────────────────────────────────────────────────

public record ObjectiveResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid OwnerId,
    Guid? TeamId,
    bool IsArchived,
    List<Guid> LinkedProcessIds);

// ── Process ──────────────────────────────────────────────────────────────────

public record ProcessResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string? SuccessMetric,
    string? TargetValue,
    DateTimeOffset? TargetDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid OwnerId,
    Guid? TeamId,
    bool IsArchived,
    List<Guid> LinkedObjectiveIds,
    List<Guid> LinkedInitiativeIds);

// ── Initiative ───────────────────────────────────────────────────────────────

public record InitiativeResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTimeOffset? TargetDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid OwnerId,
    Guid? TeamId,
    bool IsArchived,
    List<Guid> LinkedProcessIds,
    List<Guid> TaskIds);

// ── Task ─────────────────────────────────────────────────────────────────────

public record TaskItemResponse(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTimeOffset? TargetDate,
    DateTimeOffset? CompletionDate,
    decimal? EstimatedEffort,
    decimal? ActualEffort,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    Guid OwnerId,
    Guid? AssigneeId,
    Guid? TeamId,
    bool IsArchived,
    List<Guid> LinkedInitiativeIds);

// ── Hierarchy (nested) ────────────────────────────────────────────────────────

public record HierarchyTaskItem(
    Guid Id,
    string Title,
    string Status,
    decimal? EstimatedEffort,
    decimal? ActualEffort,
    Guid? AssigneeId,
    Guid? TeamId);

public record HierarchyInitiativeItem(
    Guid Id,
    string Title,
    string Status,
    DateTimeOffset? TargetDate,
    int ProgressPct,
    Guid? TeamId,
    List<HierarchyTaskItem> Tasks);

public record HierarchyProcessItem(
    Guid Id,
    string Title,
    string Status,
    DateTimeOffset? TargetDate,
    int ProgressPct,
    Guid? TeamId,
    List<HierarchyInitiativeItem> Initiatives);

public record HierarchyObjectiveItem(
    Guid Id,
    string Title,
    string Status,
    DateTimeOffset? TargetDate,
    int ProgressPct,
    Guid? TeamId,
    List<HierarchyProcessItem> Processes);
