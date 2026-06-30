using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaxOmbud.Domain.Enums;
using DomainEmail = TaxOmbud.Domain.ValueObjects.Email;
using DomainUser = TaxOmbud.Domain.Entities.Identity.User;
using TaxOmbud.Common.Responses;

namespace TaxOmbud.Application.Users.DTOs;

public record CreateUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone,
    string? JobTitle,
    string? EmploymentType,
    Guid? DepartmentId
) ;

public record CreateUserResponse(Guid Id, string FullName, string Email);