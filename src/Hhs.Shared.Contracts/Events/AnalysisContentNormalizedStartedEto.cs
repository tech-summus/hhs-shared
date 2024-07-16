using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record AnalysisContentNormalizedStartedEto(Guid TenantId, Guid ClientId, Guid AnalysisContentId, [NotNull] List<Guid> AppContentIdList, DateTime AnalysisDate) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;

    public Guid ClientId { get; } = ClientId;

    public Guid AnalysisContentId { get; } = AnalysisContentId;

    [NotNull]
    public List<Guid> AppContentIdList { get; } = AppContentIdList;

    public DateTime AnalysisDate { get; } = AnalysisDate;
}