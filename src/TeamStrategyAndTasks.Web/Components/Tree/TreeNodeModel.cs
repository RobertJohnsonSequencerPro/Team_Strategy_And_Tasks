using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Web.Components.Tree;

public record TreeNodeModel(
    Guid Id,
    string Title,
    NodeType Level,
    NodeStatus Status,
    double ProgressPct,
    bool IsShared,
    string DetailUrl,
    IReadOnlyList<TreeNodeModel> Children);
