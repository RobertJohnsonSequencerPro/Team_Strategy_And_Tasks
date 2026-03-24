namespace TeamStrategyAndTasks.Core.Entities;

public class SavedFilter : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>Human-readable label given by the user.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Identifies which list page this filter belongs to: "objectives" | "processes" | "initiatives" | "tasks".</summary>
    public string PageKey { get; set; } = string.Empty;

    /// <summary>JSON-serialized filter state. Keys: "parentId" (nullable Guid string) and "search" (string).</summary>
    public string FilterJson { get; set; } = "{}";
}
