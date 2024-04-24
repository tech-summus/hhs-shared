using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Domain.Events;

public sealed record VideoRequestSentResultEto(Guid ContentId, bool IsSendSuccess, Guid VideoRequestId) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsSendSuccess { get; } = IsSendSuccess;

    public Guid VideoRequestId { get; } = VideoRequestId;
}