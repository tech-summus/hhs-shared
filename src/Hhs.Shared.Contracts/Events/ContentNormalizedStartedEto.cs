using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedStartedEto(Guid TenantId, Guid ClientId, [NotNull] string DomainName, Guid ContentId, [NotNull] string ContentKey) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;

    public Guid ClientId { get; } = ClientId;

    [NotNull]
    public string DomainName { get; } = DomainName;

    public Guid ContentId { get; } = ContentId;

    [NotNull]
    public string ContentKey { get; } = ContentKey;
}