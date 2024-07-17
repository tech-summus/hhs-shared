using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record AppContentNormalizedResultEto(Guid AppContentId, bool IsNormalizedSuccess, Guid NormalizedRequestId,DateTime? ReleaseTime) : IIntegrationEventMessage
{
    public Guid AppContentId { get; } = AppContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedRequestId { get; } = NormalizedRequestId;

    public DateTime? ReleaseTime { get; } = ReleaseTime;
}