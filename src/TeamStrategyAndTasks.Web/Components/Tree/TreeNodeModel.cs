using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Web.Components.Tree;

public record TreeNodeModel(
    Guid Id,
    string Title,
    NodeType Level,
    NodeStatus Status,
    double ProgressPct,
    /// <summary>
    /// Number of distinct parent nodes this node is linked to across the full
    /// hierarchy. 1 = not shared; >1 = appears under multiple parents (M:M).
    /// </summary>
    int ParentCount,
    string DetailUrl,
    IReadOnlyList<TreeNodeModel> Children);
