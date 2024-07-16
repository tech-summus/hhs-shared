using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationResultEto([NotNull] string ReferenceContentType,Guid ReferenceContentId, bool IsGenerateSuccess, Guid VideoRequestId, [CanBeNull] string StorageVideoUrl) : IIntegrationEventMessage
{
    [NotNull]
    public string ReferenceContentType { get; } = ReferenceContentType;
    public Guid ReferenceContentId { get; } = ReferenceContentId;

    public bool IsGenerateSuccess { get; } = IsGenerateSuccess;

    public Guid VideoRequestId { get; } = VideoRequestId;

    [CanBeNull]
    public string StorageVideoUrl { get; } = StorageVideoUrl;
}