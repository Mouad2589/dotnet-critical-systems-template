using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Commands;

// ── Command ─────────────────────────────────────────────────────────────────

public sealed record CreateIdentityCommand(string NationalId) : IRequest<CreateIdentityResult>;

public sealed record CreateIdentityResult(Guid IdentityId, string Status);

// ── Validator ────────────────────────────────────────────────────────────────

public sealed class CreateIdentityCommandValidator : AbstractValidator<CreateIdentityCommand>
{
    public CreateIdentityCommandValidator()
    {
        RuleFor(x => x.NationalId)
            .NotEmpty()
            .Matches(@"^[A-Z0-9]{8,12}$")
            .WithMessage("National ID must be 8–12 uppercase alphanumeric characters.");
    }
}

// ── Handler ──────────────────────────────────────────────────────────────────

public sealed class CreateIdentityCommandHandler(
    IIdentityRepository repository)
    : IRequestHandler<CreateIdentityCommand, CreateIdentityResult>
{
    public async Task<CreateIdentityResult> Handle(
        CreateIdentityCommand command,
        CancellationToken cancellationToken)
    {
        var nationalId = NationalId.From(command.NationalId);

        var alreadyExists = await repository.ExistsAsync(nationalId, cancellationToken);
        if (alreadyExists)
            throw new InvalidOperationException(
                $"An identity with national ID '{command.NationalId}' already exists.");

        var identity = Identity.Create(nationalId);
        await repository.AddAsync(identity, cancellationToken);

        // Domain events are dispatched by the outbox publisher after SaveChanges.

        return new CreateIdentityResult(identity.Id, identity.Status.ToString());
    }
}
