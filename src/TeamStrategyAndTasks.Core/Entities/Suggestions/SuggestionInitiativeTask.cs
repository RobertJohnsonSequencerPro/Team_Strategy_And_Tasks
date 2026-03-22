namespace TeamStrategyAndTasks.Core.Entities.Suggestions;

public class SuggestionInitiativeTask
{
    public Guid SuggestionInitiativeId { get; set; }
    public Guid SuggestionTaskId { get; set; }

    public SuggestionInitiative SuggestionInitiative { get; set; } = null!;
    public SuggestionTask SuggestionTask { get; set; } = null!;
}
