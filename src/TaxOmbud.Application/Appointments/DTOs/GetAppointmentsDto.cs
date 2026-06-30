using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record GetAppointmentsQuery(
    Guid? TaxpayerId,
    Guid? OfficerId,
    string? Status
) ;

public record AppointmentListDto(
    Guid Id,
    string Title,
    string? Description,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string Status,
    string TaxpayerName,
    string OfficerName,
    string? Location,
    string? MeetingUrl
);