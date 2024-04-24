using HsnSoft.Base.Domain.Entities.Events;

namespace Hhs.Shared.Domain.Events;

public sealed record VideoGenerationResultEto(Guid ContentId, bool IsGenerateSuccess, Guid VideoRequestId) : IIntegrationEventMessage
{
    public Guid ContentId { get; } = ContentId;
    public bool IsGenerateSuccess { get; } = IsGenerateSuccess;

    public Guid VideoRequestId { get; } = VideoRequestId;
}