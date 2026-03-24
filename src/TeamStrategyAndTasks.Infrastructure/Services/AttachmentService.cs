using Microsoft.EntityFrameworkCore;
using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;
using TeamStrategyAndTasks.Core.Exceptions;
using TeamStrategyAndTasks.Core.Interfaces;
using TeamStrategyAndTasks.Infrastructure.Data;

namespace TeamStrategyAndTasks.Infrastructure.Services;

public class AttachmentService : IAttachmentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly string _basePath;

    public AttachmentService(IDbContextFactory<AppDbContext> dbFactory, string basePath)
    {
        _dbFactory = dbFactory;
        _basePath = basePath;
        Directory.CreateDirectory(_basePath);
    }

    public async Task<IReadOnlyList<Attachment>> GetForNodeAsync(
        NodeType nodeType, Guid nodeId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Attachments
            .AsNoTracking()
            .Where(a => a.NodeType == nodeType && a.NodeId == nodeId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Attachment> UploadAsync(
        NodeType nodeType, Guid nodeId, Guid uploaderId,
        string fileName, string contentType, long fileSize, Stream content,
        CancellationToken ct = default)
    {
        // Sanitize: keep only the file name portion — no directory components
        var safeFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(safeFileName)) safeFileName = "file";

        // Storage key is fully server-generated — no user input in the path
        var storageKey = $"{nodeType}/{nodeId:N}/{Guid.NewGuid():N}";
        var fullPath = Path.GetFullPath(Path.Combine(_basePath, storageKey));

        // Defense-in-depth: confirm resolved path is still inside the storage root
        if (!fullPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage path resolution error.");

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            await content.CopyToAsync(fs, ct);

        var attachment = new Attachment
        {
            NodeType = nodeType,
            NodeId = nodeId,
            UploaderId = uploaderId,
            FileName = safeFileName,
            StorageKey = storageKey,
            FileSizeBytes = fileSize,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        };

        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.Attachments.Add(attachment);
        await db.SaveChangesAsync(ct);
        return attachment;
    }

    public async Task DeleteAsync(Guid attachmentId, Guid requestingUserId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId, ct)
            ?? throw new NotFoundException($"Attachment {attachmentId} not found.");

        if (attachment.UploaderId != requestingUserId)
            throw new ForbiddenException("Only the uploader can delete this attachment.");

        var fullPath = Path.GetFullPath(Path.Combine(_basePath, attachment.StorageKey));
        if (!fullPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage path resolution error.");

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync(ct);
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(
        Guid attachmentId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var attachment = await db.Attachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId, ct)
            ?? throw new NotFoundException($"Attachment {attachmentId} not found.");

        var fullPath = Path.GetFullPath(Path.Combine(_basePath, attachment.StorageKey));
        if (!fullPath.StartsWith(Path.GetFullPath(_basePath), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Storage path resolution error.");

        if (!File.Exists(fullPath))
            throw new NotFoundException("Attachment file not found on disk.");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return (stream, attachment.ContentType, attachment.FileName);
    }
}
