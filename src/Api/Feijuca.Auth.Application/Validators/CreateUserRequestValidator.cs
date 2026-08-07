using Feijuca.Auth.Application.Requests.User;
using FluentValidation;

namespace Feijuca.Auth.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<AddUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("The admin username is required.")
            .EmailAddress().WithMessage("The admin username must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("The password is required.")
            .MinimumLength(6).WithMessage("The password must be at least 6 characters long.");
    }
}