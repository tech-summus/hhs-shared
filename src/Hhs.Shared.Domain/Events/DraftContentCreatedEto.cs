using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Domain.Events;

public sealed record DraftContentCreatedEto(Guid ContentId, string EncodedContentData) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public string EncodedContentData { get; } = EncodedContentData;
}