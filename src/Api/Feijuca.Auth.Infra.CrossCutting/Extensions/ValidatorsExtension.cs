using Feijuca.Auth.Application.Requests.Auth;
using Feijuca.Auth.Application.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace Feijuca.Auth.Infra.CrossCutting.Extensions
{
    public static class ValidatorsExtension
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddFluentValidationAutoValidation(x => x.DisableDataAnnotationsValidation = true);
            services.AddScoped<IValidator<LoginUserRequest>, LoginUserRequestValidator>();

            return services;
        }
    }
}
