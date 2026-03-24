namespace TeamStrategyAndTasks.Core.Entities;

public class SharedValue : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsArchived { get; set; }
}
