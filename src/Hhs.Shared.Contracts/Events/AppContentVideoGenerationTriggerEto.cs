using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Contracts.Events;

public sealed record AppContentVideoGenerationTriggerEto(Guid TenantId, Guid ClientId, Guid AppContentId) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    public Guid AppContentId { get; } = AppContentId;
}