namespace TeamStrategyAndTasks.Core.Entities.Suggestions;

public class SuggestionInitiative
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SuggestionProcessInitiative> ProcessInitiatives { get; set; } = new List<SuggestionProcessInitiative>();
    public ICollection<SuggestionInitiativeTask> InitiativeTasks { get; set; } = new List<SuggestionInitiativeTask>();
}
