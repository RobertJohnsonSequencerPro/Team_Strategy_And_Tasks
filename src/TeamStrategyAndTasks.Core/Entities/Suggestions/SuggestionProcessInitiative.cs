namespace TeamStrategyAndTasks.Core.Entities.Suggestions;

public class SuggestionProcessInitiative
{
    public Guid SuggestionProcessId { get; set; }
    public Guid SuggestionInitiativeId { get; set; }

    public SuggestionProcess SuggestionProcess { get; set; } = null!;
    public SuggestionInitiative SuggestionInitiative { get; set; } = null!;
}
