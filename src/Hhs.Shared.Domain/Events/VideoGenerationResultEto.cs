using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Domain.Events;

public sealed record VideoGenerationResultEto(Guid ContentId, bool IsGenerateSuccess, Guid VideoRequestId, [CanBeNull] string StorageVideoUrl) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsGenerateSuccess { get; } = IsGenerateSuccess;

    public Guid VideoRequestId { get; } = VideoRequestId;

    [CanBeNull]
    public string StorageVideoUrl { get; } = StorageVideoUrl;
}