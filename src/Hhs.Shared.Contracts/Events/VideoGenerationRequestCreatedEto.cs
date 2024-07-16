using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationRequestCreatedEto([NotNull] string ReferenceContentType, Guid ReferenceContentId, Guid VideoRequestId) : IIntegrationEventMessage
{
    [NotNull]
    public string ReferenceContentType { get; } = ReferenceContentType;
    public Guid ReferenceContentId { get; } = ReferenceContentId;

    public Guid VideoRequestId { get; } = VideoRequestId;
}