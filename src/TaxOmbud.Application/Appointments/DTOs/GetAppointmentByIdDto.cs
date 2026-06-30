using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record GetAppointmentByIdQuery(Guid Id) ;

public record AppointmentDetailDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    AppointmentTaxpayerDto? Taxpayer,
    AppointmentOfficerDto? Officer,
    string? Location,
    string? MeetingUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);

public record AppointmentTaxpayerDto(Guid Id, string FullName, string Email);
public record AppointmentOfficerDto(Guid Id, string FullName, string Email);