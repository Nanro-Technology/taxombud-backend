using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Entities.Identity;
using TaxOmbud.Domain.Entities.Taxpayers;
using TaxOmbud.Domain.ValueObjects;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Auth.DTOs;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber
) ;

public record RegisterResponse(Guid UserId, string Email, string FullName);