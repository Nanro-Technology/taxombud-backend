using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.System;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.System.DTOs;

public record ImpersonateUserCommand(Guid UserId) ;

public record ImpersonationResponseDto(
    string Message,
    string Token,
    Guid TargetUserId,
    Guid ImpersonatorUserId
);
