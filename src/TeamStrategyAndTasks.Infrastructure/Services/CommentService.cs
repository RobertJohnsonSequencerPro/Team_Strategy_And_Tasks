using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly INotificationService _notifications;

    public CommentService(IDbContextFactory<AppDbContext> dbFactory, INotificationService notifications)
    {
        _dbFactory = dbFactory;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<Comment>> GetForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        return await ctx.Comments
            .Where(c => c.NodeType == nodeType && c.NodeId == nodeId && c.ParentCommentId == null)
            .Include(c => c.Replies)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Comment> AddAsync(
        NodeType nodeType,
        Guid nodeId,
        Guid authorId,
        string body,
        Guid nodeOwnerId,
        Guid? parentCommentId = null,
        CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var comment = new Comment
        {
            NodeType = nodeType,
            NodeId = nodeId,
            AuthorId = authorId,
            Body = body,
            ParentCommentId = parentCommentId
        };

        ctx.Comments.Add(comment);
        await ctx.SaveChangesAsync(ct);

        var author = await ctx.Users.FindAsync(new object[] { authorId }, ct);
        var authorName = author?.DisplayName ?? "Someone";

        // Notify node owner (unless they are the author)
        if (nodeOwnerId != authorId)
        {
            await _notifications.CreateAsync(
                nodeOwnerId,
                $"{authorName} commented on your item.",
                nodeType, nodeId, ct);
        }

        // Notify any @mentioned users
        var mentions = ParseMentions(body);
        if (mentions.Count > 0)
        {
            var mentionedUsers = await ctx.Users
                .Where(u => mentions.Contains(u.NormalizedUserName!))
                .ToListAsync(ct);

            foreach (var user in mentionedUsers)
            {
                if (user.Id != authorId && user.Id != nodeOwnerId)
                {
                    await _notifications.CreateAsync(
                        user.Id,
                        $"{authorName} mentioned you in a comment.",
                        nodeType, nodeId, ct);
                }
            }
        }

        return comment;
    }

    public async Task DeleteAsync(Guid commentId, Guid requestingUserId, CancellationToken ct = default)
    {
        await using var ctx = await _dbFactory.CreateDbContextAsync(ct);

        var comment = await ctx.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId, ct)
            ?? throw new NotFoundException($"Comment {commentId} not found.");

        if (comment.AuthorId != requestingUserId)
            throw new ForbiddenException("Only the author can delete their comment.");

        ctx.Comments.RemoveRange(comment.Replies);
        ctx.Comments.Remove(comment);
        await ctx.SaveChangesAsync(ct);
    }

    private static List<string> ParseMentions(string body)
    {
        const char at = '\u0040';
        var pattern = at + @"(\w[\w._-]*)";
        return Regex.Matches(body, pattern)
            .Select(m => m.Groups[1].Value.ToUpperInvariant())
            .Distinct()
            .ToList();
    }
}
