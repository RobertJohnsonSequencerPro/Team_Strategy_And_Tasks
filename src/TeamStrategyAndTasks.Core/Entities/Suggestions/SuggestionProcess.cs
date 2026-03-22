namespace TeamStrategyAndTasks.Core.Entities.Suggestions;

public class SuggestionProcess
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SuggestionObjectiveProcess> ObjectiveProcesses { get; set; } = new List<SuggestionObjectiveProcess>();
    public ICollection<SuggestionProcessInitiative> ProcessInitiatives { get; set; } = new List<SuggestionProcessInitiative>();
}
