using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Application.Common.Interfaces;
using TaxOmbud.Application.Common.Models;
using TaxOmbud.Domain.Enums;

namespace TaxOmbud.Application.Features.Appointments.Commands.UpdateAppointmentStatus;

// ─── Command ─────────────────────────────────────────────────────────────────

public record UpdateAppointmentStatusCommand(Guid AppointmentId, string Status) : IRequest<Result<Unit>>;

// ─── Validator ────────────────────────────────────────────────────────────────

public class UpdateAppointmentStatusCommandValidator : AbstractValidator<UpdateAppointmentStatusCommand>
{
    public UpdateAppointmentStatusCommandValidator()
    {
        RuleFor(x => x.AppointmentId).NotEmpty();
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
    }
}

// ─── Handler ─────────────────────────────────────────────────────────────────

public class UpdateAppointmentStatusCommandHandler : IRequestHandler<UpdateAppointmentStatusCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppointmentStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var app = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);
        if (app == null)
            return Result<Unit>.NotFound("Appointment not found.");

        if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatus))
            return Result<Unit>.Failure($"Invalid status: '{request.Status}'.");

        app.Status = newStatus;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}
