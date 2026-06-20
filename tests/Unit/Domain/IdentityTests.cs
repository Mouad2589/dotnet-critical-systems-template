using Domain.Entities;
using Domain.Events;
using Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Unit.Domain;

public sealed class IdentityTests
{
    [Fact]
    public void Create_WithValidNationalId_ReturnsIdentityInPendingStatus()
    {
        // Arrange
        var nationalId = NationalId.From("AB12345678");

        // Act
        var identity = Identity.Create(nationalId);

        // Assert
        identity.Id.Should().NotBeEmpty();
        identity.NationalId.Should().Be(nationalId);
        identity.Status.Should().Be(IdentityStatus.PendingVerification);
    }

    [Fact]
    public void Create_WithValidNationalId_RaisesIdentityCreatedEvent()
    {
        // Arrange
        var nationalId = NationalId.From("AB12345678");

        // Act
        var identity = Identity.Create(nationalId);

        // Assert
        identity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<IdentityCreatedEvent>()
            .Which.NationalId.Should().Be(nationalId);
    }

    [Fact]
    public void Activate_WhenPending_ChangesStatusToActive()
    {
        // Arrange
        var identity = Identity.Create(NationalId.From("AB12345678"));

        // Act
        identity.Activate();

        // Assert
        identity.Status.Should().Be(IdentityStatus.Active);
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsInvalidOperationException()
    {
        // Arrange
        var identity = Identity.Create(NationalId.From("AB12345678"));
        identity.Activate();

        // Act
        var act = identity.Activate;

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*Active*");
    }

    [Fact]
    public void Suspend_WhenActive_ChangesStatusToSuspended()
    {
        // Arrange
        var identity = Identity.Create(NationalId.From("AB12345678"));
        identity.Activate();

        // Act
        identity.Suspend("Fraud investigation");

        // Assert
        identity.Status.Should().Be(IdentityStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenPending_ThrowsInvalidOperationException()
    {
        // Arrange
        var identity = Identity.Create(NationalId.From("AB12345678"));

        // Act
        var act = () => identity.Suspend("Fraud investigation");

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
