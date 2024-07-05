using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedResultEto(Guid ContentId, bool IsNormalizedSuccess, Guid NormalizedRequestId,DateTime? ReleaseTime, [CanBeNull] string EncodedNormalizedResult) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedRequestId { get; } = NormalizedRequestId;

    public DateTime? ReleaseTime { get; } = ReleaseTime;

    [CanBeNull]
    public string EncodedNormalizedResult { get; } = EncodedNormalizedResult;
}