using Feijuca.Auth.Models;

namespace Feijuca.Auth.Common.Errors
{
    public static class UserGroupErrors
    {
        public static string TechnicalMessage { get; private set; } = "";

        public static Error ErrorAddUserToGroup => new(
            "User.ErrorAddUserToGroup",
            $"An error occurred while trying adding a new user to the group: {TechnicalMessage}"
        );

        public static Error ErrorGetGroup => new(
            "Group.ErrorGetGroup",
            $"An error occurred while trying to retrieve the group: {TechnicalMessage}"
        );

        public static void SetTechnicalMessage(string message)
        {
            TechnicalMessage = message;
        }
    }
}
