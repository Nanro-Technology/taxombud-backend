using FluentValidation;
using TaxOmbud.Application.Users.DTOs;

namespace TaxOmbud.Application.Users.Validators;

public class AssignRolesCommandValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.RoleIds).NotNull();
    }
}
