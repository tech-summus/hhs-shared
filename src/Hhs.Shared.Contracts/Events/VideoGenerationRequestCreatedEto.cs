using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationRequestCreatedEto(Guid ReferenceContentId, Guid VideoRequestId) : IIntegrationEventMessage
{
    public Guid ReferenceContentId { get; } = ReferenceContentId;
    public Guid VideoRequestId { get; } = VideoRequestId;
}