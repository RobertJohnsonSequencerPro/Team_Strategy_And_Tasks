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

    // Admin management
    Task<SuggestionObjective> CreateSuggestionObjectiveAsync(string title, string? description, CancellationToken ct = default);
    Task SetSuggestionActiveAsync(string nodeType, Guid id, bool isActive, CancellationToken ct = default);
}
