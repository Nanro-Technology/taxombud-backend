using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.CreateInteraction;

public record CreateInteractionCommand : IRequest<Guid>
{
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

public class CreateInteractionCommandValidator : AbstractValidator<CreateInteractionCommand>
{
    public CreateInteractionCommandValidator()
    {
        RuleFor(v => v.Direction).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Type).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Channel).NotEmpty().MaximumLength(100);
        RuleFor(v => v.Outcome).MaximumLength(200);
        RuleFor(v => v.OccurredAt).NotEmpty();
    }
}

public class CreateInteractionCommandHandler : IRequestHandler<CreateInteractionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateInteractionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateInteractionCommand request, CancellationToken cancellationToken)
    {
        var entity = new Interaction
        {
            Direction = request.Direction,
            Subject = request.Subject,
            Type = request.Type,
            Channel = request.Channel,
            Outcome = request.Outcome,
            Notes = request.Notes,
            RelatedToId = request.RelatedToId,
            LoggedById = request.LoggedById,
            OccurredAt = request.OccurredAt
        };

        _context.Interactions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
