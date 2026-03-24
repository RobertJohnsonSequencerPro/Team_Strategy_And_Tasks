namespace TeamStrategyAndTasks.Web.Components.Tree;

/// <summary>
/// Passed as a CascadingValue from HierarchyTree down to every TreeNode.
/// Decouples inline workbench actions (edit, add child, archive) from the display component.
/// A record so that `with` expressions can be used to produce updated copies when
/// Expand All / Collapse All is triggered, causing Blazor to propagate the new
/// reference through the CascadingValue and notify all child TreeNode components.
/// </summary>
public sealed record TreeCallbacks
{
    public Func<TreeNodeModel, Task> EditNode { get; init; } = _ => Task.CompletedTask;
    public Func<TreeNodeModel, Task> AddChildToNode { get; init; } = _ => Task.CompletedTask;
    public Func<TreeNodeModel, Task> ArchiveNode { get; init; } = _ => Task.CompletedTask;

    /// <summary>Incremented each time Expand All or Collapse All is triggered.</summary>
    public int ExpandVersion { get; init; }

    /// <summary>When ExpandVersion changes, true = expand all nodes; false = collapse all.</summary>
    public bool ExpandAll { get; init; }
}
