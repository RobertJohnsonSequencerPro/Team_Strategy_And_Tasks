using TeamStrategyAndTasks.Core.Entities;
using TeamStrategyAndTasks.Core.Enums;

namespace TeamStrategyAndTasks.Core.Interfaces;

public interface IAttachmentService
{
    Task<IReadOnlyList<Attachment>> GetForNodeAsync(NodeType nodeType, Guid nodeId, CancellationToken ct = default);

    Task<Attachment> UploadAsync(
        NodeType nodeType,
        Guid nodeId,
        Guid uploaderId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken ct = default);

    Task DeleteAsync(Guid attachmentId, Guid requestingUserId, CancellationToken ct = default);

    Task<(Stream Content, string ContentType, string FileName)> DownloadAsync(Guid attachmentId, CancellationToken ct = default);
}
