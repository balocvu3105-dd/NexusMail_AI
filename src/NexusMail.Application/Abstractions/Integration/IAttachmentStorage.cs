using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NexusMail.Application.Abstractions.Integration;

public record AttachmentData(Guid Id, string Filename, Stream ContentStream);

public interface IAttachmentSource
{
    Task<IReadOnlyList<AttachmentData>> GetAttachmentsAsync(Guid workspaceId, Guid emailId, CancellationToken cancellationToken = default);
}

public interface IAttachmentStorageProvider
{
    Task<bool> SaveAsync(AttachmentData attachment, string destinationPath, string idempotencyKey, CancellationToken cancellationToken = default);
}
