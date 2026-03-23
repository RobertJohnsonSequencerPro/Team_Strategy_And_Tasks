using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Web.Components.Tree;

public static class TreeBuilder
{
    public static IReadOnlyList<TreeNodeModel> Build(IReadOnlyList<Objective> objectives)
    {
        // Detect nodes that appear under more than one parent (M:M shared nodes)
        var sharedProcessIds = objectives
            .SelectMany(o => o.ObjectiveProcesses.Select(op => op.ProcessId))
            .GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

        var sharedInitiativeIds = objectives
            .SelectMany(o => o.ObjectiveProcesses.Select(op => op.Process))
            .SelectMany(p => p.ProcessInitiatives.Select(pi => pi.InitiativeId))
            .GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

        var sharedTaskIds = objectives
            .SelectMany(o => o.ObjectiveProcesses.Select(op => op.Process))
            .SelectMany(p => p.ProcessInitiatives.Select(pi => pi.Initiative))
            .SelectMany(i => i.InitiativeWorkTasks.Select(iwt => iwt.WorkTaskId))
            .GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet();

        return objectives
            .OrderBy(o => o.Title)
            .Select(o => BuildObjective(o, sharedProcessIds, sharedInitiativeIds, sharedTaskIds))
            .ToArray();
    }

    private static TreeNodeModel BuildObjective(
        Objective obj,
        HashSet<Guid> sharedProcessIds,
        HashSet<Guid> sharedInitiativeIds,
        HashSet<Guid> sharedTaskIds)
    {
        var children = obj.ObjectiveProcesses
            .Where(op => !op.Process.IsArchived)
            .OrderBy(op => op.DisplayOrder).ThenBy(op => op.Process.Title)
            .Select(op => BuildProcess(op.Process, sharedProcessIds, sharedInitiativeIds, sharedTaskIds))
            .ToArray();

        return new TreeNodeModel(
            obj.Id, obj.Title, NodeType.Objective, obj.Status,
            ProgressFor(children), false, $"/objectives/{obj.Id}", children);
    }

    private static TreeNodeModel BuildProcess(
        BusinessProcess proc,
        HashSet<Guid> sharedProcessIds,
        HashSet<Guid> sharedInitiativeIds,
        HashSet<Guid> sharedTaskIds)
    {
        var children = proc.ProcessInitiatives
            .Where(pi => !pi.Initiative.IsArchived)
            .OrderBy(pi => pi.DisplayOrder).ThenBy(pi => pi.Initiative.Title)
            .Select(pi => BuildInitiative(pi.Initiative, sharedInitiativeIds, sharedTaskIds))
            .ToArray();

        return new TreeNodeModel(
            proc.Id, proc.Title, NodeType.Process, proc.Status,
            ProgressFor(children), sharedProcessIds.Contains(proc.Id), $"/processes/{proc.Id}", children);
    }

    private static TreeNodeModel BuildInitiative(
        Initiative init,
        HashSet<Guid> sharedInitiativeIds,
        HashSet<Guid> sharedTaskIds)
    {
        var children = init.InitiativeWorkTasks
            .Where(iwt => !iwt.WorkTask.IsArchived)
            .OrderBy(iwt => iwt.DisplayOrder).ThenBy(iwt => iwt.WorkTask.Title)
            .Select(iwt => BuildTask(iwt.WorkTask, sharedTaskIds))
            .ToArray();

        return new TreeNodeModel(
            init.Id, init.Title, NodeType.Initiative, init.Status,
            ProgressFor(children), sharedInitiativeIds.Contains(init.Id), $"/initiatives/{init.Id}", children);
    }

    private static TreeNodeModel BuildTask(WorkTask task, HashSet<Guid> sharedTaskIds) =>
        new(task.Id, task.Title, NodeType.Task, task.Status,
            task.Status == NodeStatus.Done ? 100.0 : 0.0,
            sharedTaskIds.Contains(task.Id), $"/tasks/{task.Id}", []);

    // Progress for a non-task node = average of children's progress percentages.
    // If no children are linked, reports 0%.
    private static double ProgressFor(IReadOnlyList<TreeNodeModel> children) =>
        children.Count > 0 ? children.Average(c => c.ProgressPct) : 0.0;
}
