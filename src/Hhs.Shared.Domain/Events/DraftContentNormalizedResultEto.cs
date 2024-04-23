using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Domain.Events;

public sealed record DraftContentNormalizedResultEto(Guid ContentId, bool IsNormalizedSuccess, string NormalizedResult) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsNormalizedSuccess { get; } = IsNormalizedSuccess;
    public string NormalizedResult { get; } = NormalizedResult;
}