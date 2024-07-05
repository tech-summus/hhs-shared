using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentVideoGenerationTriggerEto(Guid TenantId, Guid ClientId, Guid ContentId) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;
    public Guid ClientId { get; } = ClientId;
    public Guid ContentId { get; } = ContentId;
}