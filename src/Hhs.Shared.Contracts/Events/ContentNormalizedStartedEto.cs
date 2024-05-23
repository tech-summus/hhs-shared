using HsnSoft.Base.Domain.Entities.Events;
using JetBrains.Annotations;

namespace Hhs.Shared.Contracts.Events;

public sealed record ContentNormalizedStartedEto(Guid TenantId, Guid ClientId, Guid ContentId, [NotNull] string DomainName, [NotNull] string DomainPath) : IIntegrationEventMessage
{
    public Guid TenantId { get; } = TenantId;

    public Guid ClientId { get; } = ClientId;

    public Guid ContentId { get; } = ContentId;

    [NotNull]
    public string DomainName { get; } = DomainName;

    [NotNull]
    public string DomainPath { get; } = DomainPath;
}