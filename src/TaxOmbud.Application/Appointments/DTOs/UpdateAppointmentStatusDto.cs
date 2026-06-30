using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Appointments.DTOs;

public record UpdateAppointmentStatusCommand(Guid AppointmentId, string Status) ;

public record UpdateAppointmentStatusRequest(string Status);