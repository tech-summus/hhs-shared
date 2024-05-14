using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedResultEto(Guid ContentId, bool IsNormalizedSuccess, Guid NormalizedRequestId, [CanBeNull] string EncodedNormalizedResult) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedRequestId { get; } = NormalizedRequestId;

    [CanBeNull]
    public string EncodedNormalizedResult { get; } = EncodedNormalizedResult;
}