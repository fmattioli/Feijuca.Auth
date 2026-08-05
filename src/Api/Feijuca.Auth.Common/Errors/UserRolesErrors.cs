using Feijuca.Auth.Models;

namespace Feijuca.Auth.Common.Errors;

public static class UserRolesErrors
{
    public static string TechnicalMessage { get; private set; } = "";

    public static Error ErrorGetUserRoles => new(
        "UserRoles.ErrorGetUserRoles",
        $"An error occurred while trying to get user roles: {TechnicalMessage}"
    );

    public static Error ErrorAddRoleToUser => new(
        "UserRoles.ErrorAddRoleToUser",
        $"An error occurred while trying to add a role to the user: {TechnicalMessage}"
    );

    public static Error RemovingRoleFromUserError => new(
        "UserRoles.RemovingRoleFromUserError",
        $"An error occurred while trying to remove the role from the user: {TechnicalMessage}"
    );
}
