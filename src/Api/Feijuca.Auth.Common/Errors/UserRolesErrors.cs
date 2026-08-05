using Feijuca.Auth.Models;

namespace Feijuca.Auth.Common.Errors;

public static class UserRolesErrors
{
    public static string TechnicalMessage { get; private set; } = "";

    public static Error ErrorGetUserRoles => new(
            "UserRoles.ErrorGetUserRoles",
            $"An error occurred while trying to get user roles: {TechnicalMessage}"
        );
}
