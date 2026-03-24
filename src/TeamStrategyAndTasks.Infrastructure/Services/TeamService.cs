using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.DTOs;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class TeamService(AppDbContext db) : ITeamService
{
    public async Task<IReadOnlyList<Team>> GetAllAsync(CancellationToken ct = default) =>
        await db.Teams
            .Where(t => !t.IsArchived)
            .Include(t => t.TeamMembers)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

    public async Task<Team> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var team = await db.Teams
            .Include(t => t.TeamMembers)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
        return team ?? throw new NotFoundException(nameof(Team), id);
    }

    public async Task<Team> CreateAsync(CreateTeamRequest request, CancellationToken ct = default)
    {
        var team = new Team
        {
            Name = request.Name,
            Mandate = request.Mandate
        };
        db.Teams.Add(team);
        await db.SaveChangesAsync(ct);
        return team;
    }

    public async Task<Team> UpdateAsync(Guid id, UpdateTeamRequest request, CancellationToken ct = default)
    {
        var team = await GetByIdAsync(id, ct);
        team.Name = request.Name;
        team.Mandate = request.Mandate;
        await db.SaveChangesAsync(ct);
        return team;
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var team = await GetByIdAsync(id, ct);
        team.IsArchived = true;
        await db.SaveChangesAsync(ct);
    }

    public async Task AddMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        var alreadyMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId, ct);
        if (alreadyMember) return;

        db.TeamMembers.Add(new TeamMember { TeamId = teamId, UserId = userId });
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid userId, CancellationToken ct = default)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId, ct);
        if (member is not null)
        {
            db.TeamMembers.Remove(member);
            await db.SaveChangesAsync(ct);
        }
    }
}
