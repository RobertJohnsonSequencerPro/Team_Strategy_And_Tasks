using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class NodeDependencyService(IDbContextFactory<AppDbContext> dbFactory) : INodeDependencyService
{
    // ── Query ────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<NodeDependencyDto>> GetBlockersForAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var deps = await db.NodeDependencies
            .Where(d => d.BlockedType == nodeType && d.BlockedId == nodeId)
            .ToListAsync(ct);

        return await ToDtosAsync(db, deps, ct);
    }

    public async Task<IReadOnlyList<NodeDependencyDto>> GetBlockedByThisAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var deps = await db.NodeDependencies
            .Where(d => d.BlockerType == nodeType && d.BlockerId == nodeId)
            .ToListAsync(ct);

        return await ToDtosAsync(db, deps, ct);
    }

    // ── Mutations ────────────────────────────────────────────────────────────

    public async Task<NodeDependencyDto> AddAsync(
        AddDependencyRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
            await using var db = await dbFactory.CreateDbContextAsync(ct);
        if (request.BlockerType == request.BlockedType && request.BlockerId == request.BlockedId)
            throw new AppValidationException("Dependency", "A node cannot depend on itself.");

        var duplicate = await db.NodeDependencies.AnyAsync(d =>
            d.BlockerType == request.BlockerType && d.BlockerId == request.BlockerId &&
            d.BlockedType == request.BlockedType && d.BlockedId == request.BlockedId, ct);

        if (duplicate)
            throw new AppValidationException("Dependency", "This dependency already exists.");

        // Cycle detection: starting from the *blocked* node following the outgoing
        // "blocks" direction, can we reach the blocker? If yes → cycle.
        if (await WouldCreateCycleAsync(
                db, request.BlockedType, request.BlockedId,
                request.BlockerType, request.BlockerId, ct))
            throw new AppValidationException("Dependency",
                "Adding this dependency would create a circular dependency chain.");

        var dep = new NodeDependency
        {
            BlockerType      = request.BlockerType,
            BlockerId        = request.BlockerId,
            BlockedType      = request.BlockedType,
            BlockedId        = request.BlockedId,
            DependencyType   = request.DependencyType,
            Notes            = request.Notes,
            CreatedByUserId  = performedByUserId
        };

        db.NodeDependencies.Add(dep);
        await db.SaveChangesAsync(ct);

        // Immediately propagate blocked status for the affected node
        await TrySetBlockedAsync(db, dep.BlockedType, dep.BlockedId, ct);

        return await ToDtoAsync(db, dep, ct);
    }

    public async Task RemoveAsync(Guid dependencyId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var dep = await db.NodeDependencies.FindAsync([dependencyId], ct)
            ?? throw new NotFoundException($"Dependency {dependencyId} not found.");

        db.NodeDependencies.Remove(dep);
        await db.SaveChangesAsync(ct);
    }

    // ── Blocked-status propagation ───────────────────────────────────────────

    public async Task PropagateBlockedStatusAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var allDeps = await db.NodeDependencies.ToListAsync(ct);

        foreach (var dep in allDeps)
        {
            var blockerDone = await IsNodeTerminalAsync(db, dep.BlockerType, dep.BlockerId, ct);
            if (!blockerDone)
                await TrySetBlockedAsync(db, dep.BlockedType, dep.BlockedId, ct);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<NodeDependencyDto>> ToDtosAsync(
        AppDbContext db, IEnumerable<NodeDependency> deps, CancellationToken ct)
    {
        var result = new List<NodeDependencyDto>();
        foreach (var d in deps)
            result.Add(await ToDtoAsync(db, d, ct));
        return result;
    }

    private static async Task<NodeDependencyDto> ToDtoAsync(AppDbContext db, NodeDependency d, CancellationToken ct)
    {
        var blockerTitle = await GetTitleAsync(db, d.BlockerType, d.BlockerId, ct);
        var blockedTitle = await GetTitleAsync(db, d.BlockedType, d.BlockedId, ct);
            return new NodeDependencyDto(
            d.Id,
            d.BlockerType, d.BlockerId, blockerTitle,
            d.BlockedType, d.BlockedId, blockedTitle,
            d.DependencyType, d.Notes);
    }

    private static async Task<string> GetTitleAsync(AppDbContext db, NodeType type, Guid id, CancellationToken ct) =>
        type switch
        {
            NodeType.Objective  => await db.Objectives.Where(x => x.Id == id).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? "(unknown)",
            NodeType.Process    => await db.BusinessProcesses.Where(x => x.Id == id).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? "(unknown)",
            NodeType.Initiative => await db.Initiatives.Where(x => x.Id == id).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? "(unknown)",
            NodeType.Task       => await db.WorkTasks.Where(x => x.Id == id).Select(x => x.Title).FirstOrDefaultAsync(ct) ?? "(unknown)",
            _                   => "(unknown)"
        };

    private static async Task<bool> IsNodeTerminalAsync(AppDbContext db, NodeType type, Guid id, CancellationToken ct)
    {
        var terminal = new[] { NodeStatus.Done, NodeStatus.Complete, NodeStatus.Cancelled, NodeStatus.Archived };
        return type switch
        {
            NodeType.Objective  => await db.Objectives.AnyAsync(x => x.Id == id && terminal.Contains(x.Status), ct),
            NodeType.Process    => await db.BusinessProcesses.AnyAsync(x => x.Id == id && terminal.Contains(x.Status), ct),
            NodeType.Initiative => await db.Initiatives.AnyAsync(x => x.Id == id && terminal.Contains(x.Status), ct),
            NodeType.Task       => await db.WorkTasks.AnyAsync(x => x.Id == id && terminal.Contains(x.Status), ct),
            _                   => false
        };
    }

    private static readonly NodeStatus[] AutoStatuses =
    [
        NodeStatus.NotStarted, NodeStatus.Active, NodeStatus.InProgress, NodeStatus.OnTrack
    ];

    private static async Task TrySetBlockedAsync(AppDbContext db, NodeType type, Guid id, CancellationToken ct)
    {
        switch (type)
        {
            case NodeType.Objective:
                var obj = await db.Objectives.FindAsync([id], ct);
                if (obj is not null && AutoStatuses.Contains(obj.Status))
                { obj.Status = NodeStatus.Blocked; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Process:
                var proc = await db.BusinessProcesses.FindAsync([id], ct);
                if (proc is not null && AutoStatuses.Contains(proc.Status))
                { proc.Status = NodeStatus.Blocked; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Initiative:
                var init = await db.Initiatives.FindAsync([id], ct);
                if (init is not null && AutoStatuses.Contains(init.Status))
                { init.Status = NodeStatus.Blocked; await db.SaveChangesAsync(ct); }
                break;
            case NodeType.Task:
                var task = await db.WorkTasks.FindAsync([id], ct);
                if (task is not null && AutoStatuses.Contains(task.Status))
                { task.Status = NodeStatus.Blocked; await db.SaveChangesAsync(ct); }
                break;
        }
    }

    /// <summary>
    /// BFS from <paramref name="startType"/>/<paramref name="startId"/> following the
    /// outgoing "blocks" direction. Returns true if <paramref name="targetType"/>/<paramref
    /// name="targetId"/> is reachable — which would indicate a cycle.
    /// </summary>
    private static async Task<bool> WouldCreateCycleAsync(
        AppDbContext db,
        NodeType startType, Guid startId,
        NodeType targetType, Guid targetId,
        CancellationToken ct)
    {
        var visited = new HashSet<(NodeType, Guid)>();
        var queue   = new Queue<(NodeType, Guid)>();
        queue.Enqueue((startType, startId));

        while (queue.Count > 0)
        {
            var (curType, curId) = queue.Dequeue();
            if (!visited.Add((curType, curId))) continue;

            if (curType == targetType && curId == targetId) return true;

            var next = await db.NodeDependencies
                .Where(d => d.BlockerType == curType && d.BlockerId == curId)
                .Select(d => new { d.BlockedType, d.BlockedId })
                .ToListAsync(ct);

            foreach (var n in next)
                queue.Enqueue((n.BlockedType, n.BlockedId));
        }

        return false;
    }
}
