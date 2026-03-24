using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;
using TeamStrategyAndTasks.Infrastructure.Identity;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class WorkloadService(
    IDbContextFactory<AppDbContext> dbFactory) : IWorkloadService
{
    // Statuses that mean a node is no longer actively worked on
    private static readonly NodeStatus[] InactiveHierarchyStatuses =
        [NodeStatus.Complete, NodeStatus.Archived];

    private static readonly NodeStatus[] InactiveDoneOrCancelled =
        [NodeStatus.Done, NodeStatus.Cancelled, NodeStatus.Archived];

    // ── Public API ─────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<UserWorkloadRow>> GetUserWorkloadsAsync(
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var now        = DateTimeOffset.UtcNow;
        var weekAhead  = now.AddDays(7);

        // ── Flat projections of each entity type ──────────────────────────
        var objectives = await db.Objectives
            .AsNoTracking()
            .Where(x => !x.IsArchived && !InactiveHierarchyStatuses.Contains(x.Status))
            .Select(x => new NodeSlim(x.OwnerId, null, x.TargetDate, x.Status))
            .ToListAsync(ct);

        var processes = await db.BusinessProcesses
            .AsNoTracking()
            .Where(x => !x.IsArchived && !InactiveHierarchyStatuses.Contains(x.Status))
            .Select(x => new NodeSlim(x.OwnerId, null, x.TargetDate, x.Status))
            .ToListAsync(ct);

        var initiatives = await db.Initiatives
            .AsNoTracking()
            .Where(x => !x.IsArchived && !InactiveHierarchyStatuses.Contains(x.Status))
            .Select(x => new NodeSlim(x.OwnerId, null, x.TargetDate, x.Status))
            .ToListAsync(ct);

        var tasks = await db.WorkTasks
            .AsNoTracking()
            .Where(x => !x.IsArchived)
            .Select(x => new NodeSlim(x.OwnerId, x.AssigneeId, x.TargetDate, x.Status))
            .ToListAsync(ct);

        // ── Collect all user IDs involved ─────────────────────────────────
        var allNodes   = objectives.Concat(processes).Concat(initiatives).ToList();
        var activeTasks = tasks.Where(t => !InactiveDoneOrCancelled.Contains(t.Status)).ToList();

        var ownerIds = allNodes.Select(n => n.OwnerId)
            .Concat(activeTasks.Select(t => t.OwnerId))
            .Concat(activeTasks.Where(t => t.AssigneeId.HasValue).Select(t => t.AssigneeId!.Value))
            .Distinct()
            .ToHashSet();

        if (ownerIds.Count == 0) return [];

        var userNames = await db.Users
            .AsNoTracking()
            .Where(u => ownerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? u.UserName ?? u.Id.ToString(), ct);

        // ── Aggregate per user ─────────────────────────────────────────────
        var result = new List<UserWorkloadRow>();

        foreach (var userId in ownerIds)
        {
            // Hierarchy nodes this user owns
            var ownedHierarchy = allNodes.Where(n => n.OwnerId == userId).ToList();

            // Tasks where user is owner or assignee (deduplicated by identity)
            var ownedTasks   = activeTasks.Where(t => t.OwnerId == userId).ToList();
            var assignedTasks = activeTasks.Where(t => t.AssigneeId == userId && t.OwnerId != userId).ToList();
            var userActiveTasks = ownedTasks.Concat(assignedTasks).ToList();

            var totalActive = ownedHierarchy.Count + userActiveTasks.Count;
            var overdue     = ownedHierarchy.Count(n => n.TargetDate.HasValue && n.TargetDate < now)
                            + userActiveTasks.Count(t => t.TargetDate.HasValue && t.TargetDate < now);
            var dueSoon     = ownedHierarchy.Count(n => n.TargetDate.HasValue && n.TargetDate >= now && n.TargetDate <= weekAhead)
                            + userActiveTasks.Count(t => t.TargetDate.HasValue && t.TargetDate >= now && t.TargetDate <= weekAhead);

            // Task completion — all non-archived tasks assigned to user (includes done/cancelled as numerator denominator)
            var allUserTasks = tasks.Where(t => t.OwnerId == userId || t.AssigneeId == userId)
                                    .Where(t => !InactiveDoneOrCancelled.Contains(t.Status) || t.Status == NodeStatus.Done)
                                    .ToList();
            var doneTasks = allUserTasks.Count(t => t.Status == NodeStatus.Done);
            var pct       = allUserTasks.Count > 0 ? (double)doneTasks / allUserTasks.Count * 100 : 0;

            result.Add(new UserWorkloadRow(
                userId,
                userNames.GetValueOrDefault(userId, userId.ToString()),
                totalActive,
                overdue,
                dueSoon,
                Math.Round(pct, 1)));
        }

        return result.OrderByDescending(r => r.TotalActiveNodes).ThenBy(r => r.UserName).ToList();
    }

    public async Task<IReadOnlyList<TeamWorkloadRow>> GetTeamWorkloadsAsync(
        CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);

        var teams = await db.Teams
            .AsNoTracking()
            .Where(t => !t.IsArchived)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        if (teams.Count == 0) return [];

        var teamMembers = await db.TeamMembers
            .AsNoTracking()
            .Select(tm => new { tm.TeamId, tm.UserId })
            .ToListAsync(ct);

        var userWorkloads = await GetUserWorkloadsAsync(ct);
        var userIndex     = userWorkloads.ToDictionary(u => u.UserId);

        var result = new List<TeamWorkloadRow>();

        foreach (var team in teams)
        {
            var memberIds = teamMembers.Where(tm => tm.TeamId == team.Id).Select(tm => tm.UserId).ToList();
            if (memberIds.Count == 0) continue;

            var memberRows = memberIds
                .Where(id => userIndex.ContainsKey(id))
                .Select(id => userIndex[id])
                .ToList();

            var totalActive = memberRows.Sum(r => r.TotalActiveNodes);
            var overdue     = memberRows.Sum(r => r.OverdueCount);
            var dueSoon     = memberRows.Sum(r => r.DueThisWeekCount);
            var avgPct      = memberRows.Count > 0 ? memberRows.Average(r => r.TaskCompletionPct) : 0;

            result.Add(new TeamWorkloadRow(
                team.Id,
                team.Name,
                memberIds.Count,
                totalActive,
                overdue,
                dueSoon,
                Math.Round(avgPct, 1)));
        }

        return result.OrderByDescending(r => r.TotalActiveNodes).ThenBy(r => r.TeamName).ToList();
    }

    // ── Private helpers ────────────────────────────────────────────────────

    private record NodeSlim(Guid OwnerId, Guid? AssigneeId, DateTimeOffset? TargetDate, NodeStatus Status);
}
