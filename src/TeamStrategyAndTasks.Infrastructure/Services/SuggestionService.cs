using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities.Suggestions;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class SuggestionService(IDbContextFactory<AppDbContext> dbFactory) : ISuggestionService
{
    public async Task<IReadOnlyList<SuggestionObjective>> GetSuggestedObjectivesAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionObjectives
            .Where(s => s.IsActive)
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionProcess>> GetSuggestedProcessesForObjectiveAsync(
        Guid suggestionObjectiveId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionObjectiveProcesses
            .Where(op => op.SuggestionObjectiveId == suggestionObjectiveId)
            .Select(op => op.SuggestionProcess)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionInitiative>> GetSuggestedInitiativesForProcessAsync(
        Guid suggestionProcessId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionProcessInitiatives
            .Where(pi => pi.SuggestionProcessId == suggestionProcessId)
            .Select(pi => pi.SuggestionInitiative)
            .Where(i => i.IsActive)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionTask>> GetSuggestedTasksForInitiativeAsync(
        Guid suggestionInitiativeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionInitiativeTasks
            .Where(it => it.SuggestionInitiativeId == suggestionInitiativeId)
            .Select(it => it.SuggestionTask)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionProcess>> GetAllSuggestedProcessesAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionProcesses
            .Where(p => p.IsActive)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionInitiative>> GetAllSuggestedInitiativesAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionInitiatives
            .Where(i => i.IsActive)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionTask>> GetAllSuggestedTasksAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionTasks
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task<SuggestionObjective> CreateSuggestionObjectiveAsync(
        string title, string? description, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var suggestion = new SuggestionObjective { Title = title, Description = description };
        db.SuggestionObjectives.Add(suggestion);
        await db.SaveChangesAsync(ct);
        return suggestion;
    }

    public async Task<SuggestionProcess> CreateSuggestionProcessAsync(
        string title, string? description, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var proc = new SuggestionProcess { Title = title, Description = description };
        db.SuggestionProcesses.Add(proc);
        await db.SaveChangesAsync(ct);
        return proc;
    }

    public async Task<SuggestionInitiative> CreateSuggestionInitiativeAsync(
        string title, string? description, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var init = new SuggestionInitiative { Title = title, Description = description };
        db.SuggestionInitiatives.Add(init);
        await db.SaveChangesAsync(ct);
        return init;
    }

    public async Task<SuggestionTask> CreateSuggestionTaskAsync(
        string title, string? description, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var task = new SuggestionTask { Title = title, Description = description };
        db.SuggestionTasks.Add(task);
        await db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<IReadOnlyList<SuggestionObjective>> GetAllObjectivesAdminAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionObjectives
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionProcess>> GetAllProcessesAdminAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionProcesses
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionInitiative>> GetAllInitiativesAdminAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionInitiatives
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionTask>> GetAllTasksAdminAsync(CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionTasks
            .OrderBy(s => s.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionProcess>> GetLinkedProcessesForObjectiveAdminAsync(
        Guid objectiveId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionObjectiveProcesses
            .Where(op => op.SuggestionObjectiveId == objectiveId)
            .Select(op => op.SuggestionProcess)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionInitiative>> GetLinkedInitiativesForProcessAdminAsync(
        Guid processId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionProcessInitiatives
            .Where(pi => pi.SuggestionProcessId == processId)
            .Select(pi => pi.SuggestionInitiative)
            .OrderBy(i => i.Title)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SuggestionTask>> GetLinkedTasksForInitiativeAdminAsync(
        Guid initiativeId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        return await db.SuggestionInitiativeTasks
            .Where(it => it.SuggestionInitiativeId == initiativeId)
            .Select(it => it.SuggestionTask)
            .OrderBy(t => t.Title)
            .ToListAsync(ct);
    }

    public async Task UpdateSuggestionAsync(string nodeType, Guid id, string title, string? description, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        switch (nodeType.ToLowerInvariant())
        {
            case "objective":
                var obj = await db.SuggestionObjectives.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionObjective", id);
                obj.Title = title; obj.Description = description;
                break;
            case "process":
                var proc = await db.SuggestionProcesses.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionProcess", id);
                proc.Title = title; proc.Description = description;
                break;
            case "initiative":
                var init = await db.SuggestionInitiatives.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionInitiative", id);
                init.Title = title; init.Description = description;
                break;
            case "task":
                var task = await db.SuggestionTasks.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionTask", id);
                task.Title = title; task.Description = description;
                break;
            default:
                throw new ArgumentException($"Unknown suggestion node type: {nodeType}");
        }
        await db.SaveChangesAsync(ct);
    }

    public async Task SetObjectiveProcessLinksAsync(Guid objectiveId, IEnumerable<Guid> processIds, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var existing = await db.SuggestionObjectiveProcesses
            .Where(op => op.SuggestionObjectiveId == objectiveId)
            .ToListAsync(ct);
        db.SuggestionObjectiveProcesses.RemoveRange(existing);

        foreach (var pid in processIds)
            db.SuggestionObjectiveProcesses.Add(new() { SuggestionObjectiveId = objectiveId, SuggestionProcessId = pid });

        await db.SaveChangesAsync(ct);
    }

    public async Task SetProcessInitiativeLinksAsync(Guid processId, IEnumerable<Guid> initiativeIds, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var existing = await db.SuggestionProcessInitiatives
            .Where(pi => pi.SuggestionProcessId == processId)
            .ToListAsync(ct);
        db.SuggestionProcessInitiatives.RemoveRange(existing);

        foreach (var iid in initiativeIds)
            db.SuggestionProcessInitiatives.Add(new() { SuggestionProcessId = processId, SuggestionInitiativeId = iid });

        await db.SaveChangesAsync(ct);
    }

    public async Task SetInitiativeTaskLinksAsync(Guid initiativeId, IEnumerable<Guid> taskIds, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var existing = await db.SuggestionInitiativeTasks
            .Where(it => it.SuggestionInitiativeId == initiativeId)
            .ToListAsync(ct);
        db.SuggestionInitiativeTasks.RemoveRange(existing);

        foreach (var tid in taskIds)
            db.SuggestionInitiativeTasks.Add(new() { SuggestionInitiativeId = initiativeId, SuggestionTaskId = tid });

        await db.SaveChangesAsync(ct);
    }

    public async Task SetSuggestionActiveAsync(string nodeType, Guid id, bool isActive, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        switch (nodeType.ToLowerInvariant())
        {
            case "objective":
                var obj = await db.SuggestionObjectives.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionObjective", id);
                obj.IsActive = isActive;
                break;
            case "process":
                var proc = await db.SuggestionProcesses.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionProcess", id);
                proc.IsActive = isActive;
                break;
            case "initiative":
                var init = await db.SuggestionInitiatives.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionInitiative", id);
                init.IsActive = isActive;
                break;
            case "task":
                var task = await db.SuggestionTasks.FindAsync([id], ct)
                    ?? throw new NotFoundException("SuggestionTask", id);
                task.IsActive = isActive;
                break;
            default:
                throw new ArgumentException($"Unknown suggestion node type: {nodeType}");
        }
        await db.SaveChangesAsync(ct);
    }
}
