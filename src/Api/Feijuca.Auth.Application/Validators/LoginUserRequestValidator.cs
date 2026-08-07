using Feijuca.Auth.Application.Requests.Auth;
using FluentValidation;

namespace Feijuca.Auth.Application.Validators
{
    public class LoginUserRequestValidator : AbstractValidator<LoginUserRequest>
    {
        public LoginUserRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The {nameof(LoginUserRequest.Username)} is mandatory.")
                .EmailAddress()
                .WithMessage("The admin username must be a valid email address.");

            RuleFor(x => x.Password)
                .NotNull()
                .NotEmpty()
                .WithMessage($"The {nameof(LoginUserRequest.Password)} is mandatory.");
        }
    }
}