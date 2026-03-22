using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities.Suggestions;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class SuggestionService(AppDbContext db) : ISuggestionService
{
    public Task<IReadOnlyList<SuggestionObjective>> GetSuggestedObjectivesAsync(CancellationToken ct = default) =>
        db.SuggestionObjectives
            .Where(s => s.IsActive)
            .OrderBy(s => s.Title)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<SuggestionObjective>)t.Result, ct);

    public Task<IReadOnlyList<SuggestionProcess>> GetSuggestedProcessesForObjectiveAsync(
        Guid suggestionObjectiveId, CancellationToken ct = default) =>
        db.SuggestionObjectiveProcesses
            .Where(op => op.SuggestionObjectiveId == suggestionObjectiveId)
            .Select(op => op.SuggestionProcess)
            .Where(p => p.IsActive)
            .OrderBy(p => p.Title)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<SuggestionProcess>)t.Result, ct);

    public Task<IReadOnlyList<SuggestionInitiative>> GetSuggestedInitiativesForProcessAsync(
        Guid suggestionProcessId, CancellationToken ct = default) =>
        db.SuggestionProcessInitiatives
            .Where(pi => pi.SuggestionProcessId == suggestionProcessId)
            .Select(pi => pi.SuggestionInitiative)
            .Where(i => i.IsActive)
            .OrderBy(i => i.Title)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<SuggestionInitiative>)t.Result, ct);

    public Task<IReadOnlyList<SuggestionTask>> GetSuggestedTasksForInitiativeAsync(
        Guid suggestionInitiativeId, CancellationToken ct = default) =>
        db.SuggestionInitiativeTasks
            .Where(it => it.SuggestionInitiativeId == suggestionInitiativeId)
            .Select(it => it.SuggestionTask)
            .Where(t => t.IsActive)
            .OrderBy(t => t.Title)
            .ToListAsync(ct)
            .ContinueWith(t => (IReadOnlyList<SuggestionTask>)t.Result, ct);

    public async Task<SuggestionObjective> CreateSuggestionObjectiveAsync(
        string title, string? description, CancellationToken ct = default)
    {
        var suggestion = new SuggestionObjective { Title = title, Description = description };
        db.SuggestionObjectives.Add(suggestion);
        await db.SaveChangesAsync(ct);
        return suggestion;
    }

    public async Task SetSuggestionActiveAsync(string nodeType, Guid id, bool isActive, CancellationToken ct = default)
    {
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
