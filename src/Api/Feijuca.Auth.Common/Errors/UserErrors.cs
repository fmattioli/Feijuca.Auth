using Feijuca.Auth.Models;

namespace Feijuca.Auth.Common.Errors
{
    public static class UserErrors
    {
        public static string TechnicalMessage { get; private set; } = "";

        public static Error TokenGenerationError => new(
            "User.TokenGeneration",
            $"An error occurred while trying to receive information about realm: {TechnicalMessage}"
        );

        public static Error InvalidUserNameOrPasswordError => new(
            "User.InvalidUserNameOrPassword",
            $"An error occurred while trying to get JWT token. Please check username and password. {TechnicalMessage}"
        );

        public static Error InvalidRefreshToken => new(
            "User.InvalidRefreshTokenProvided",
            $"An error occurred while trying to refresh token. {TechnicalMessage}"
        );

        public static Error InvalidId => new(
            "User.InvalidId",
            $"An error occurred while trying to get the user. Please check the user ID. {TechnicalMessage}"
        );

        public static Error SignOutError => new(
            "User.SignOutError",
            $"An error occurred while trying to do a sign out {TechnicalMessage}"
        );

        public static Error UserCreationError => new(
            "User.UserCreationError",
            $"An error occurred while trying create a user. {TechnicalMessage}"
        );

        public static Error WrongPasswordDefinition => new(
            "User.WrongPasswordDefinition",
            $"An error occurred while trying to add a new password to the user. {TechnicalMessage}"
        );

        public static Error GetAllUsersError => new(
            "User.GetAllUsersError",
            $"An error occurred while trying get all users. {TechnicalMessage}"
        );

        public static Error GetUserError => new(
            "User.GetUserError",
            $"An error occurred while trying get user. {TechnicalMessage}"
        );

        public static Error DeletionUserError => new(
            "Group.DeletionUserError",
            $"An error occurred while trying delete the user: {TechnicalMessage}"
        );

        public static Error RevokeSessionsError => new(
            "User.RevokeSessionsError",
            $"An error occurred while trying to revoke sessions for the user: {TechnicalMessage}"
        );

        public static Error ErrorWhileAddedUserAttribute => new(
            "User.ErrorWhileAddedUserAttribute",
            $"An error occurred while trying add an attribute to the user. {TechnicalMessage}"
        );

        public static Error GetUserSessionsError => new(
            "User.GetUserSessionsError",
            $"An error occurred while trying to get sessions an the user. {TechnicalMessage}"
        );
        public static Error ResetPasswordError => new(
            "User.ResetPassword",
            $"An error occurred while updating the user's password. {TechnicalMessage}"
        );

        public static void SetTechnicalMessage(string technicalMessage)
        {
            TechnicalMessage = technicalMessage;
        }
    }

}