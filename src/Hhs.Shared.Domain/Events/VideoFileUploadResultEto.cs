using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Domain.Events;

public sealed record VideoFileUploadResultEto(Guid ContentId, bool IsFileUploadSuccess, [CanBeNull] string StorageVideoUrl) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsFileUploadSuccess { get; } = IsFileUploadSuccess;

    [CanBeNull]
    public string StorageVideoUrl { get; } = StorageVideoUrl;
}