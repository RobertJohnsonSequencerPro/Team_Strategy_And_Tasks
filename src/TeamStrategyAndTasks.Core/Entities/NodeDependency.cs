namespace TeamStrategyAndTasks.Core.Entities;

/// <summary>
/// Records a blocking relationship between two nodes at any level of the hierarchy.
/// Semantics: <see cref="BlockerId" /> (of <see cref="BlockerType" />) must reach a terminal
/// state before <see cref="BlockedId" /> (of <see cref="BlockedType" />) can proceed.
/// </summary>
public class NodeDependency : BaseEntity
{
    public NodeType BlockerType { get; set; }
    public Guid    BlockerId   { get; set; }

    public NodeType BlockedType { get; set; }
    public Guid     BlockedId   { get; set; }

    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

    public string? Notes { get; set; }

    public Guid CreatedByUserId { get; set; }
}
