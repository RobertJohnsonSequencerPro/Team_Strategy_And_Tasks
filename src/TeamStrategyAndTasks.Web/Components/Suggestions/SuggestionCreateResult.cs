namespace TeamStrategyAndTasks.Web.Components.Suggestions;

/// <summary>Wraps a create-mode dialog result so calling pages can handle optional cascade children.</summary>
public record SuggestionCreateResult<TRequest>(
    TRequest Request,
    IReadOnlyList<CascadeItem> CascadeItems);
