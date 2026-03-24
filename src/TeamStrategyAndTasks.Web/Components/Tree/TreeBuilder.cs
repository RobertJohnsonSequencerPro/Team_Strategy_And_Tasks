using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Web.Components.Tree;

public static class TreeBuilder
{
    public static IReadOnlyList<TreeNodeModel> Build(IReadOnlyList<Objective> objectives)
    {
        // Count how many distinct parent nodes each node is linked to across the
        // full hierarchy.  A count > 1 means the node is shared (M:M relationship).

        // Process parent count = distinct Objectives per Process.
        var processParentCounts = objectives
            .SelectMany(o => o.ObjectiveProcesses.Select(op => (ObjectiveId: o.Id, ProcessId: op.ProcessId)))
            .GroupBy(x => x.ProcessId, x => x.ObjectiveId)
            .ToDictionary(g => g.Key, g => g.Distinct().Count());

        // De-duplicate processes before counting initiative parents to avoid
        // inflating counts when a process appears under multiple objectives.
        var allProcesses = objectives
            .SelectMany(o => o.ObjectiveProcesses.Select(op => op.Process))
            .DistinctBy(p => p.Id)
            .ToList();

        // Initiative parent count = distinct Processes per Initiative.
        var initiativeParentCounts = allProcesses
            .SelectMany(p => p.ProcessInitiatives.Select(pi => (ProcessId: p.Id, InitiativeId: pi.InitiativeId)))
            .GroupBy(x => x.InitiativeId, x => x.ProcessId)
            .ToDictionary(g => g.Key, g => g.Distinct().Count());

        // Task parent count = distinct Initiatives per Task.
        var allInitiatives = allProcesses
            .SelectMany(p => p.ProcessInitiatives.Select(pi => pi.Initiative))
            .DistinctBy(i => i.Id)
            .ToList();

        var taskParentCounts = allInitiatives
            .SelectMany(i => i.InitiativeWorkTasks.Select(iwt => (InitiativeId: i.Id, TaskId: iwt.WorkTaskId)))
            .GroupBy(x => x.TaskId, x => x.InitiativeId)
            .ToDictionary(g => g.Key, g => g.Distinct().Count());

        return objectives
            .OrderBy(o => o.Title)
            .Select(o => BuildObjective(o, processParentCounts, initiativeParentCounts, taskParentCounts))
            .ToArray();
    }

    private static TreeNodeModel BuildObjective(
        Objective obj,
        Dictionary<Guid, int> processParentCounts,
        Dictionary<Guid, int> initiativeParentCounts,
        Dictionary<Guid, int> taskParentCounts)
    {
        var children = obj.ObjectiveProcesses
            .Where(op => !op.Process.IsArchived)
            .OrderBy(op => op.DisplayOrder).ThenBy(op => op.Process.Title)
            .Select(op => BuildProcess(op.Process, processParentCounts, initiativeParentCounts, taskParentCounts))
            .ToArray();

        // Objectives are always roots — ParentCount is always 1.
        return new TreeNodeModel(
            obj.Id, obj.Title, NodeType.Objective, obj.Status,
            ProgressFor(children), 1, $"/objectives/{obj.Id}", children);
    }

    private static TreeNodeModel BuildProcess(
        BusinessProcess proc,
        Dictionary<Guid, int> processParentCounts,
        Dictionary<Guid, int> initiativeParentCounts,
        Dictionary<Guid, int> taskParentCounts)
    {
        var children = proc.ProcessInitiatives
            .Where(pi => !pi.Initiative.IsArchived)
            .OrderBy(pi => pi.DisplayOrder).ThenBy(pi => pi.Initiative.Title)
            .Select(pi => BuildInitiative(pi.Initiative, initiativeParentCounts, taskParentCounts))
            .ToArray();

        return new TreeNodeModel(
            proc.Id, proc.Title, NodeType.Process, proc.Status,
            ProgressFor(children),
            processParentCounts.GetValueOrDefault(proc.Id, 1),
            $"/processes/{proc.Id}", children);
    }

    private static TreeNodeModel BuildInitiative(
        Initiative init,
        Dictionary<Guid, int> initiativeParentCounts,
        Dictionary<Guid, int> taskParentCounts)
    {
        var children = init.InitiativeWorkTasks
            .Where(iwt => !iwt.WorkTask.IsArchived)
            .OrderBy(iwt => iwt.DisplayOrder).ThenBy(iwt => iwt.WorkTask.Title)
            .Select(iwt => BuildTask(iwt.WorkTask, taskParentCounts))
            .ToArray();

        return new TreeNodeModel(
            init.Id, init.Title, NodeType.Initiative, init.Status,
            ProgressFor(children),
            initiativeParentCounts.GetValueOrDefault(init.Id, 1),
            $"/initiatives/{init.Id}", children);
    }

    private static TreeNodeModel BuildTask(WorkTask task, Dictionary<Guid, int> taskParentCounts) =>
        // Per style guide §8: Done and Cancelled both count as 100% for rollup.
        new(task.Id, task.Title, NodeType.Task, task.Status,
            task.Status is NodeStatus.Done or NodeStatus.Cancelled ? 100.0 : 0.0,
            taskParentCounts.GetValueOrDefault(task.Id, 1), $"/tasks/{task.Id}", []);

    // Progress for a non-task node = average of all children's progress percentages.
    // Returns 0% when no children are linked (empty branch).
    private static double ProgressFor(IReadOnlyList<TreeNodeModel> children) =>
        children.Count > 0 ? children.Average(c => c.ProgressPct) : 0.0;
}
