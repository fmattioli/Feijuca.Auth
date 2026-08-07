using Feijuca.Auth.Application.Requests.Config;
using FluentValidation;

namespace Feijuca.Auth.Application.Validators;

public class AddKeycloakSettingsRequestValidator : AbstractValidator<AddKeycloakSettingsRequest>
{
    public AddKeycloakSettingsRequestValidator()
    {
        RuleFor(x => x.RealmAdminUser)
            .NotNull()
            .WithMessage($"The {nameof(AddKeycloakSettingsRequest.RealmAdminUser)} is mandatory.")
            .SetValidator(new LoginUserRequestValidator());
    }
}