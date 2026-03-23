namespace TeamStrategyAndTasks.Web.Components.Suggestions;

public record SuggestionAdoptedArgs(
    string Title,
    string? Description,
    IReadOnlyList<CascadeItem> CascadeItems);
