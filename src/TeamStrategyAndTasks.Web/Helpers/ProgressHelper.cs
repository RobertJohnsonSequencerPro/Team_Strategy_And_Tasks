using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Web.Helpers;

public static class ProgressHelper
{
    public static bool IsTaskDone(WorkTask t) =>
        t.Status is NodeStatus.Done or NodeStatus.Complete;

    /// <summary>Task completion 0–100.</summary>
    public static int TaskProgress(WorkTask t) => IsTaskDone(t) ? 100 : 0;

    /// <summary>Initiative progress 0–100 based on linked tasks.
    /// Returns null when there are no linked tasks (no data yet).</summary>
    public static int? InitiativeProgress(Initiative i)
    {
        var tasks = i.InitiativeWorkTasks
            .Where(iwt => iwt.WorkTask is not null && !iwt.WorkTask.IsArchived)
            .Select(iwt => iwt.WorkTask!)
            .ToList();
        if (tasks.Count == 0) return null;
        return (int)Math.Round(tasks.Average(t => (double)TaskProgress(t)));
    }

    /// <summary>Process progress 0–100 based on linked initiatives.
    /// Returns null when there are no linked initiatives, or all initiatives have no tasks.</summary>
    public static int? ProcessProgress(BusinessProcess p)
    {
        var inits = p.ProcessInitiatives
            .Where(pi => pi.Initiative is not null && !pi.Initiative.IsArchived)
            .Select(pi => pi.Initiative!)
            .ToList();
        if (inits.Count == 0) return null;
        var scores = inits.Select(i => InitiativeProgress(i)).OfType<int>().ToList();
        if (scores.Count == 0) return null;
        return (int)Math.Round(scores.Average());
    }

    /// <summary>Objective progress 0–100 based on linked processes.
    /// Returns null when there are no linked processes, or no data flows in.</summary>
    public static int? ObjectiveProgress(Objective o)
    {
        var procs = o.ObjectiveProcesses
            .Where(op => op.Process is not null && !op.Process.IsArchived)
            .Select(op => op.Process!)
            .ToList();
        if (procs.Count == 0) return null;
        var scores = procs.Select(p => ProcessProgress(p)).OfType<int>().ToList();
        if (scores.Count == 0) return null;
        return (int)Math.Round(scores.Average());
    }

    /// <summary>Pick a MudBlazor Color for a progress value.</summary>
    public static MudBlazor.Color ProgressColor(int? pct) => pct switch
    {
        null => MudBlazor.Color.Default,
        100 => MudBlazor.Color.Success,
        >= 70 => MudBlazor.Color.Info,
        >= 30 => MudBlazor.Color.Warning,
        _ => MudBlazor.Color.Error
    };
}
