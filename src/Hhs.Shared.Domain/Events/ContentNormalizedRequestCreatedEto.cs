using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Domain.Events;

public sealed record ContentNormalizedRequestCreatedEto(Guid ReferenceContentId, Guid NormalizedRequestId) : IIntegrationEventMessage
{
    public Guid ReferenceContentId { get; } = ReferenceContentId;
    public Guid NormalizedRequestId { get; } = NormalizedRequestId;
}