using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Appointments;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record BookAppointmentCommand(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
) ;

public record BookAppointmentResponse(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);

public record BookAppointmentRequest(
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Location,
    string? MeetingUrl
);