using HsnSoft.Base.Domain.Entities.Events;

namespace Hosting.Domain.Events;

public record ShipmentStartedEto(Guid OrderId, Guid ShipmentId) : IIntegrationEventMessage
{
    public Guid OrderId { get; } = OrderId;
    public Guid ShipmentId { get; } = ShipmentId;
}