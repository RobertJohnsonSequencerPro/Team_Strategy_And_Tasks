using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities.Suggestions;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface ISuggestionService
{
    Task<IReadOnlyList<SuggestionObjective>> GetSuggestedObjectivesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionProcess>> GetSuggestedProcessesForObjectiveAsync(Guid suggestionObjectiveId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionInitiative>> GetSuggestedInitiativesForProcessAsync(Guid suggestionProcessId, CancellationToken ct = default);
    Task<IReadOnlyList<SuggestionTask>> GetSuggestedTasksForInitiativeAsync(Guid suggestionInitiativeId, CancellationToken ct = default);

    // Admin management
    Task<SuggestionObjective> CreateSuggestionObjectiveAsync(string title, string? description, CancellationToken ct = default);
    Task SetSuggestionActiveAsync(string nodeType, Guid id, bool isActive, CancellationToken ct = default);
}
