using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record UpdateCurrentUserCommand(
    string FirstName,
    string LastName,
    string? Phone,
    string? JobTitle
) ;

public record UpdateCurrentUserRequest(string FirstName, string LastName, string? Phone, string? JobTitle);