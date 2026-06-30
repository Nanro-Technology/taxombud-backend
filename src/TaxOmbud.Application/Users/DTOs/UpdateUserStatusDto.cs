using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record UpdateUserStatusCommand(Guid Id, bool Activate) ;

public record UpdateUserStatusRequest(bool Activate);