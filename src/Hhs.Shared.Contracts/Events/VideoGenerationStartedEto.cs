using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationStartedEto(Guid TenantId, Guid ClientId, [NotNull] string ReferenceContentType,Guid ReferenceContentId, [NotNull] string EncodedNormalizedContentData) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;

    [NotNull]
    public string ReferenceContentType { get; } = ReferenceContentType;
    public Guid ReferenceContentId { get; } = ReferenceContentId;

    [NotNull]
    public string EncodedNormalizedContentData { get; } = EncodedNormalizedContentData;
}