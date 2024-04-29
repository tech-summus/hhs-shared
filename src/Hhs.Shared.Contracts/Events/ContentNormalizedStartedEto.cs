using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedStartedEto(Guid ClientId, Guid ContentId, [NotNull] string EncodedContentData) : IIntegrationEventMessage
{
    public Guid ClientId { get; } = ClientId;
    public Guid ContentId { get; } = ContentId;

    [NotNull]
    public string EncodedContentData { get; } = EncodedContentData;
}