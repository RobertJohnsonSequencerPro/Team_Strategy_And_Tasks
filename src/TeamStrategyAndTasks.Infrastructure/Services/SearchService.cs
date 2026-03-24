using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Interfaces;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class SearchService(AppDbContext db) : ISearchService
{
    private const int MaxPerType = 20;

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var qLower = query.ToLower();

        var objectives = await db.Objectives
            .Where(x => !x.IsArchived &&
                        (x.Title.ToLower().Contains(qLower) ||
                         (x.Description != null && x.Description.ToLower().Contains(qLower))))
            .OrderBy(x => x.Title)
            .Take(MaxPerType)
            .Select(x => new SearchResult(NodeType.Objective, x.Id, x.Title, x.Description, x.Status))
            .ToListAsync(ct);

        var processes = await db.BusinessProcesses
            .Where(x => !x.IsArchived &&
                        (x.Title.ToLower().Contains(qLower) ||
                         (x.Description != null && x.Description.ToLower().Contains(qLower))))
            .OrderBy(x => x.Title)
            .Take(MaxPerType)
            .Select(x => new SearchResult(NodeType.Process, x.Id, x.Title, x.Description, x.Status))
            .ToListAsync(ct);

        var initiatives = await db.Initiatives
            .Where(x => !x.IsArchived &&
                        (x.Title.ToLower().Contains(qLower) ||
                         (x.Description != null && x.Description.ToLower().Contains(qLower))))
            .OrderBy(x => x.Title)
            .Take(MaxPerType)
            .Select(x => new SearchResult(NodeType.Initiative, x.Id, x.Title, x.Description, x.Status))
            .ToListAsync(ct);

        var tasks = await db.WorkTasks
            .Where(x => !x.IsArchived &&
                        (x.Title.ToLower().Contains(qLower) ||
                         (x.Description != null && x.Description.ToLower().Contains(qLower))))
            .OrderBy(x => x.Title)
            .Take(MaxPerType)
            .Select(x => new SearchResult(NodeType.Task, x.Id, x.Title, x.Description, x.Status))
            .ToListAsync(ct);

        return [.. objectives, .. processes, .. initiatives, .. tasks];
    }
}
