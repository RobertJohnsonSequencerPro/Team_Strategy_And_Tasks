using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class ProcessService(AppDbContext db, IAuditService audit) : IProcessService
{
    public async Task<IReadOnlyList<BusinessProcess>> GetAllAsync(CancellationToken ct = default) =>
        await db.BusinessProcesses
            .Where(p => !p.IsArchived)
            .Include(p => p.ObjectiveProcesses).ThenInclude(op => op.Objective)
            .Include(p => p.ProcessInitiatives).ThenInclude(pi => pi.Initiative)
            .OrderBy(p => p.Title)
            .ToListAsync(ct);

    public async Task<BusinessProcess> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var process = await db.BusinessProcesses
            .Include(p => p.ObjectiveProcesses).ThenInclude(op => op.Objective)
            .Include(p => p.ProcessInitiatives).ThenInclude(pi => pi.Initiative)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        return process ?? throw new NotFoundException(nameof(BusinessProcess), id);
    }

    public async Task<BusinessProcess> CreateAsync(CreateProcessRequest request, Guid ownerId, CancellationToken ct = default)
    {
        var process = new BusinessProcess
        {
            Title = request.Title,
            Description = request.Description,
            OwnerId = ownerId,
            SuccessMetric = request.SuccessMetric,
            TargetValue = request.TargetValue,
            TargetDate = request.TargetDate
        };
        db.BusinessProcesses.Add(process);
        await db.SaveChangesAsync(ct);
        return process;
    }

    public async Task<BusinessProcess> UpdateAsync(Guid id, UpdateProcessRequest request, Guid performedByUserId, CancellationToken ct = default)
    {
        var process = await GetByIdAsync(id, ct);
        var logs = new List<(string Field, string? Old, string? New)>();
        if (process.Title != request.Title) logs.Add(("Title", process.Title, request.Title));
        if (process.Description != request.Description) logs.Add(("Description", process.Description, request.Description));
        if (process.SuccessMetric != request.SuccessMetric) logs.Add(("SuccessMetric", process.SuccessMetric, request.SuccessMetric));
        if (process.TargetValue != request.TargetValue) logs.Add(("TargetValue", process.TargetValue, request.TargetValue));
        if (process.TargetDate != request.TargetDate) logs.Add(("TargetDate", process.TargetDate?.ToString("o"), request.TargetDate?.ToString("o")));
        if (process.Status != request.Status) logs.Add(("Status", process.Status.ToString(), request.Status.ToString()));

        process.Title = request.Title;
        process.Description = request.Description;
        process.SuccessMetric = request.SuccessMetric;
        process.TargetValue = request.TargetValue;
        process.TargetDate = request.TargetDate;
        process.Status = request.Status;
        await db.SaveChangesAsync(ct);

        foreach (var (field, old, next) in logs)
            await audit.LogAsync(NodeType.Process, id, performedByUserId, field, old, next, ct);

        return process;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var process = await GetByIdAsync(id, ct);
        process.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task LinkInitiativeAsync(Guid processId, Guid initiativeId, CancellationToken ct = default)
    {
        var alreadyLinked = await db.ProcessInitiatives
            .AnyAsync(pi => pi.ProcessId == processId && pi.InitiativeId == initiativeId, ct);
        if (alreadyLinked) return;

        db.ProcessInitiatives.Add(new ProcessInitiative { ProcessId = processId, InitiativeId = initiativeId });
        await db.SaveChangesAsync(ct);
    }

    public async Task UnlinkInitiativeAsync(Guid processId, Guid initiativeId, CancellationToken ct = default)
    {
        var link = await db.ProcessInitiatives
            .FirstOrDefaultAsync(pi => pi.ProcessId == processId && pi.InitiativeId == initiativeId, ct);
        if (link is not null)
        {
            db.ProcessInitiatives.Remove(link);
            await db.SaveChangesAsync(ct);
        }
    }
}
