using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationStartedEto(Guid TenantId, Guid ClientId, Guid ContentId, [NotNull] string EncodedNormalizedContentData) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    public Guid ContentId { get; } = ContentId;

    [NotNull]
    public string EncodedNormalizedContentData { get; } = EncodedNormalizedContentData;
}