using Domain.ValueObjects;

namespace Domain.Events;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

public sealed record IdentityCreatedEvent(
    Guid IdentityId,
    NationalId NationalId,
    Guid EventId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public IdentityCreatedEvent(Guid identityId, NationalId nationalId)
        : this(identityId, nationalId, Guid.NewGuid(), DateTimeOffset.UtcNow) { }
}

public sealed record IdentityActivatedEvent(
    Guid IdentityId,
    Guid EventId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public IdentityActivatedEvent(Guid identityId)
        : this(identityId, Guid.NewGuid(), DateTimeOffset.UtcNow) { }
}

public sealed record IdentitySuspendedEvent(
    Guid IdentityId,
    string Reason,
    Guid EventId,
    DateTimeOffset OccurredAt) : IDomainEvent
{
    public IdentitySuspendedEvent(Guid identityId, string reason)
        : this(identityId, reason, Guid.NewGuid(), DateTimeOffset.UtcNow) { }
}
