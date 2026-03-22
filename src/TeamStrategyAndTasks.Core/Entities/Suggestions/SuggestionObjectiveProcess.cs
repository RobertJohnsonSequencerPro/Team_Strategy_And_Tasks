namespace TeamStrategyAndTasks.Core.Entities.Suggestions;

public class SuggestionObjectiveProcess
{
    public Guid SuggestionObjectiveId { get; set; }
    public Guid SuggestionProcessId { get; set; }

    public SuggestionObjective SuggestionObjective { get; set; } = null!;
    public SuggestionProcess SuggestionProcess { get; set; } = null!;
}
