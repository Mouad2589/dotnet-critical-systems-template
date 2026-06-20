namespace Domain.Entities;

/// <summary>
/// Identity aggregate root.
/// Represents a single verified identity record in the system.
/// </summary>
public sealed class Identity
{
    public Guid Id { get; private set; }
    public NationalId NationalId { get; private set; }
    public IdentityStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Identity() { } // EF Core

    public static Identity Create(NationalId nationalId)
    {
        ArgumentNullException.ThrowIfNull(nationalId);

        var identity = new Identity
        {
            Id         = Guid.NewGuid(),
            NationalId = nationalId,
            Status     = IdentityStatus.PendingVerification,
            CreatedAt  = DateTimeOffset.UtcNow,
            UpdatedAt  = DateTimeOffset.UtcNow
        };

        identity._domainEvents.Add(new IdentityCreatedEvent(identity.Id, nationalId));
        return identity;
    }

    public void Activate()
    {
        if (Status != IdentityStatus.PendingVerification)
            throw new InvalidOperationException(
                $"Cannot activate an identity in status '{Status}'.");

        Status    = IdentityStatus.Active;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new IdentityActivatedEvent(Id));
    }

    public void Suspend(string reason)
    {
        if (Status != IdentityStatus.Active)
            throw new InvalidOperationException(
                $"Cannot suspend an identity in status '{Status}'.");

        Status    = IdentityStatus.Suspended;
        UpdatedAt = DateTimeOffset.UtcNow;
        _domainEvents.Add(new IdentitySuspendedEvent(Id, reason));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
