using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record AnalysisContentNormalizedResultEto(Guid AnalysisContentId, bool IsNormalizedSuccess, Guid NormalizedAnalysisId, [CanBeNull] string EncodedNormalizedResult) : IIntegrationEventMessage
{
    public Guid AnalysisContentId { get; } = AnalysisContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedAnalysisId { get; } = NormalizedAnalysisId;

    [CanBeNull]
    public string EncodedNormalizedResult { get; } = EncodedNormalizedResult;
}