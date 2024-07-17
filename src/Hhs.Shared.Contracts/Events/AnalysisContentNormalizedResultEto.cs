using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record AnalysisContentNormalizedResultEto(Guid AnalysisContentId, bool IsNormalizedSuccess, Guid NormalizedAnalysisId) : IIntegrationEventMessage
{
    public Guid AnalysisContentId { get; } = AnalysisContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;

    public Guid NormalizedAnalysisId { get; } = NormalizedAnalysisId;
}