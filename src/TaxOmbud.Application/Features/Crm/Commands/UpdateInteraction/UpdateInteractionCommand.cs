using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Domain.Exceptions;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.UpdateInteraction;

public record UpdateInteractionCommand : IRequest
{
    public Guid Id { get; init; }
    public string Direction { get; init; } = null!;
    public string Subject { get; init; } = null!;
    public string Type { get; init; } = null!;
    public string Channel { get; init; } = null!;
    public string? Outcome { get; init; }
    public string? Notes { get; init; }
    public Guid? RelatedToId { get; init; }
    public Guid? LoggedById { get; init; }
    public DateTime OccurredAt { get; init; }
}

public class UpdateInteractionCommandValidator : AbstractValidator<UpdateInteractionCommand>
{
    public UpdateInteractionCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Direction).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Channel).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Outcome).MaximumLength(200);
        RuleFor(v => v.OccurredAt).NotEmpty();
    }
}

public class UpdateInteractionCommandHandler : IRequestHandler<UpdateInteractionCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateInteractionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateInteractionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Interactions.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(Interaction), request.Id);
        }

        entity.Direction = request.Direction;
        entity.Subject = request.Subject;
        entity.Type = request.Type;
        entity.Channel = request.Channel;
        entity.Outcome = request.Outcome;
        entity.Notes = request.Notes;
        entity.RelatedToId = request.RelatedToId;
        entity.LoggedById = request.LoggedById;
        entity.OccurredAt = request.OccurredAt;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
