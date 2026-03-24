using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities.Suggestions;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ISuggestionService
{
    Task<IReadOnlyList<SuggestionObjective>> GetSuggestedObjectivesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionProcess>> GetSuggestedProcessesForObjectiveAsync(Guid suggestionObjectiveId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionInitiative>> GetSuggestedInitiativesForProcessAsync(Guid suggestionProcessId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionTask>> GetSuggestedTasksForInitiativeAsync(Guid suggestionInitiativeId, CancellationToken ct = default);

    // Flat (unfiltered by parent) — used by suggestion panel when no parent context is known
    Task<IReadOnlyList<SuggestionProcess>> GetAllSuggestedProcessesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionInitiative>> GetAllSuggestedInitiativesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionTask>> GetAllSuggestedTasksAsync(CancellationToken ct = default);

    // Admin management — read (all including inactive)
    Task<IReadOnlyList<SuggestionObjective>> GetAllObjectivesAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionProcess>> GetAllProcessesAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionInitiative>> GetAllInitiativesAdminAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionTask>> GetAllTasksAdminAsync(CancellationToken ct = default);

    // Admin management — get linked children (all, including inactive)
    Task<IReadOnlyList<SuggestionProcess>> GetLinkedProcessesForObjectiveAdminAsync(Guid objectiveId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionInitiative>> GetLinkedInitiativesForProcessAdminAsync(Guid processId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionTask>> GetLinkedTasksForInitiativeAdminAsync(Guid initiativeId, CancellationToken ct = default);

    // Admin management — create
    Task<SuggestionObjective> CreateSuggestionObjectiveAsync(string title, string? description, CancellationToken ct = default);
    Task<SuggestionProcess> CreateSuggestionProcessAsync(string title, string? description, CancellationToken ct = default);
    Task<SuggestionInitiative> CreateSuggestionInitiativeAsync(string title, string? description, CancellationToken ct = default);
    Task<SuggestionTask> CreateSuggestionTaskAsync(string title, string? description, CancellationToken ct = default);

    // Admin management — update / toggle active
    Task UpdateSuggestionAsync(string nodeType, Guid id, string title, string? description, CancellationToken ct = default);
    Task SetSuggestionActiveAsync(string nodeType, Guid id, bool isActive, CancellationToken ct = default);

    // Admin management — relationship management (replaces all existing links)
    Task SetObjectiveProcessLinksAsync(Guid objectiveId, IEnumerable<Guid> processIds, CancellationToken ct = default);
    Task SetProcessInitiativeLinksAsync(Guid processId, IEnumerable<Guid> initiativeIds, CancellationToken ct = default);
    Task SetInitiativeTaskLinksAsync(Guid initiativeId, IEnumerable<Guid> taskIds, CancellationToken ct = default);
}
