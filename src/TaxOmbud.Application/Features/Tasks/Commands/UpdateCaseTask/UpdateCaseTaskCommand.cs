using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Domain.Entities.Cases;
using TaxOmbud.Domain.Exceptions;

namespace TaxOmbud.Application.Features.Tasks.Commands.UpdateCaseTask;

public record UpdateCaseTaskCommand : IRequest
{
    public Guid Id { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public string Status { get; init; } = null!;
    public string Priority { get; init; } = null!;
    public DateTimeOffset? DueAt { get; init; }
    public Guid? AssignedToId { get; init; }
    public Guid? LinkedCaseId { get; init; }
}

public class UpdateCaseTaskCommandValidator : AbstractValidator<UpdateCaseTaskCommand>
{
    public UpdateCaseTaskCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty().WithMessage("Id is required.");
        
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
            
        RuleFor(v => v.Status)
            .NotEmpty().WithMessage("Status is required.")
            .MaximumLength(50).WithMessage("Status must not exceed 50 characters.");
            
        RuleFor(v => v.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .MaximumLength(50).WithMessage("Priority must not exceed 50 characters.");
    }
}

public class UpdateCaseTaskCommandHandler : IRequestHandler<UpdateCaseTaskCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCaseTaskCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCaseTaskCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.CaseTasks.FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(CaseTask), request.Id);
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.Status = request.Status;
        entity.Priority = request.Priority;
        entity.DueAt = request.DueAt;
        entity.AssignedToId = request.AssignedToId;
        entity.LinkedCaseId = request.LinkedCaseId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
