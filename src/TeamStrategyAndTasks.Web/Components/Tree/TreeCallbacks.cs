namespace TeamStrategyAndTasks.Web.Components.Tree;

/// <summary>
/// Passed as a CascadingValue from HierarchyTree down to every TreeNode.
/// Decouples inline workbench actions (edit, add child, archive) from the display component.
/// </summary>
public sealed class TreeCallbacks
{
    public Func<TreeNodeModel, Task> EditNode { get; init; } = _ => Task.CompletedTask;
    public Func<TreeNodeModel, Task> AddChildToNode { get; init; } = _ => Task.CompletedTask;
    public Func<TreeNodeModel, Task> ArchiveNode { get; init; } = _ => Task.CompletedTask;
}
