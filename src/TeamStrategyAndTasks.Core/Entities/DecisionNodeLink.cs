using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>Join entity that links a Decision to a hierarchy node.</summary>
public class DecisionNodeLink
{
    public Guid DecisionId { get; set; }
    public Decision Decision { get; set; } = null!;

    public NodeType NodeType { get; set; }
    public Guid NodeId { get; set; }
}
