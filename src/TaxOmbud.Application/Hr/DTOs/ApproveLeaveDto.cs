using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Hr.DTOs;

public record ApproveLeaveCommand(Guid Id, bool Approved, string? SupervisorNote) ;

public record ApproveLeaveRequest(bool Approved, string? SupervisorNote);
