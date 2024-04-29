using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationStartedEto(Guid ClientId, Guid ContentId, [NotNull] string NormalizedContentData) : IIntegrationEventMessage
{
    public Guid ClientId { get; } = ClientId;
    public Guid ContentId { get; } = ContentId;

    [NotNull]
    public string NormalizedContentData { get; } = NormalizedContentData;
}