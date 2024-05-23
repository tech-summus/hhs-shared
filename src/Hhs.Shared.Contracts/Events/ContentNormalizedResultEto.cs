using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedResultEto(Guid ContentId, bool IsNormalizedSuccess, Guid NormalizedRequestId,DateTime? ReleaseDate, [CanBeNull] string EncodedNormalizedResult) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedRequestId { get; } = NormalizedRequestId;

    public DateTime? ReleaseDate { get; } = ReleaseDate;

    [CanBeNull]
    public string EncodedNormalizedResult { get; } = EncodedNormalizedResult;
}