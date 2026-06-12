using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Crm;

namespace TaxOmbud.Application.Features.Crm.Commands.CreateCall;

public record CreateCallCommand : IRequest<Guid>
{
    public string Subject { get; init; } = null!;
    public string? CallerType { get; init; }
    public string? CallerMethod { get; init; }
    public string? CallerIdentifier { get; init; }
    public string? CalleeMethod { get; init; }
    public string? CalleeIdentifier { get; init; }
    public string Direction { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string Phone { get; init; } = null!;
    public string? Notes { get; init; }
    public Guid? LinkedToId { get; init; }
    public Guid? AgentId { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
}

public class CreateCallCommandValidator : AbstractValidator<CreateCallCommand>
{
    public CreateCallCommandValidator()
    {
        RuleFor(v => v.Subject).NotEmpty().MaximumLength(200);
        RuleFor(v => v.Direction).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Status).NotEmpty().MaximumLength(50);
        RuleFor(v => v.Phone).NotEmpty().MaximumLength(50);
    }
}

public class CreateCallCommandHandler : IRequestHandler<CreateCallCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateCallCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateCallCommand request, CancellationToken cancellationToken)
    {
        var entity = new Call
        {
            Subject = request.Subject,
            CallerType = request.CallerType,
            CallerMethod = request.CallerMethod,
            CallerIdentifier = request.CallerIdentifier,
            CalleeMethod = request.CalleeMethod,
            CalleeIdentifier = request.CalleeIdentifier,
            Direction = request.Direction,
            Status = request.Status,
            Phone = request.Phone,
            Notes = request.Notes,
            LinkedToId = request.LinkedToId,
            AgentId = request.AgentId,
            StartAt = request.StartAt,
            EndAt = request.EndAt
        };

        _context.Calls.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
