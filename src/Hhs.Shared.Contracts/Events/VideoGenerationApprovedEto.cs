using Hhs.Shared.Contracts.Enums;
using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Contracts.Events;

public sealed record VideoGenerationApprovedEto(Guid TenantId, Guid ClientId, ReferenceContentTypes ReferenceContentType, Guid ReferenceContentId) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;

    public ReferenceContentTypes ReferenceContentType { get; } = ReferenceContentType;
    public Guid ReferenceContentId { get; } = ReferenceContentId;
}