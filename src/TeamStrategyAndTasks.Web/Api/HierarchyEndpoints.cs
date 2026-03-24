using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Web.Api;

public static class HierarchyEndpoints
{
    public static IEndpointRouteBuilder MapApiHierarchyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/hierarchy", async (IObjectiveService svc) =>
        {
            var objectives = await svc.GetFullHierarchyAsync();
            var result = objectives.Select(BuildObjective).ToList();
            return Results.Ok(result);
        })
        .WithTags("Hierarchy")
        .WithSummary("Return the full strategy hierarchy as a nested tree")
        .RequireAuthorization("ApiBearer")
        .Produces<IEnumerable<HierarchyObjectiveItem>>();

        return app;
    }

    private static HierarchyObjectiveItem BuildObjective(Objective o)
    {
        var processes = o.ObjectiveProcesses
            .Select(op => op.Process)
            .Where(p => p is not null)
            .Select(p => BuildProcess(p!))
            .ToList();

        var progress = processes.Count > 0
            ? (int)Math.Round(processes.Average(p => p.ProgressPct))
            : StatusToProgress(o.Status);

        return new HierarchyObjectiveItem(
            o.Id,
            o.Title,
            o.Status.ToString(),
            o.TargetDate,
            progress,
            o.TeamId,
            processes);
    }

    private static HierarchyProcessItem BuildProcess(BusinessProcess p)
    {
        var initiatives = p.ProcessInitiatives
            .Select(pi => pi.Initiative)
            .Where(i => i is not null)
            .Select(i => BuildInitiative(i!))
            .ToList();

        var progress = initiatives.Count > 0
            ? (int)Math.Round(initiatives.Average(i => i.ProgressPct))
            : StatusToProgress(p.Status);

        return new HierarchyProcessItem(
            p.Id,
            p.Title,
            p.Status.ToString(),
            p.TargetDate,
            progress,
            p.TeamId,
            initiatives);
    }

    private static HierarchyInitiativeItem BuildInitiative(Initiative i)
    {
        var tasks = i.InitiativeWorkTasks
            .Select(iwt => iwt.WorkTask)
            .Where(t => t is not null)
            .Select(t => BuildTask(t!))
            .ToList();

        var progress = tasks.Count > 0
            ? (int)Math.Round(tasks.Average(t => StatusToProgress(
                Enum.TryParse<NodeStatus>(t.Status, out var s) ? s : NodeStatus.NotStarted)))
            : StatusToProgress(i.Status);

        return new HierarchyInitiativeItem(
            i.Id,
            i.Title,
            i.Status.ToString(),
            i.TargetDate,
            progress,
            i.TeamId,
            tasks);
    }

    private static HierarchyTaskItem BuildTask(WorkTask t) => new(
        t.Id,
        t.Title,
        t.Status.ToString(),
        t.EstimatedEffort,
        t.ActualEffort,
        t.AssigneeId,
        t.TeamId);

    private static int StatusToProgress(NodeStatus status) => status switch
    {
        NodeStatus.Done or NodeStatus.Complete => 100,
        NodeStatus.InProgress or NodeStatus.Active or
            NodeStatus.OnTrack or NodeStatus.AtRisk or NodeStatus.Blocked => 50,
        _ => 0
    };
}
