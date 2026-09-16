using Feijuca.Auth.Models;

namespace Feijuca.Auth.Common.Errors
{
    public static class RealmErrors
    {
        public static string TechnicalMessage { get; private set; } = "";

        public static void SetTechnicalMessage(string technicalMessage)
        {
            TechnicalMessage = technicalMessage;
        }

        public static Error CreateRealmError => new(
            "Realm.CreateRealmError",
            $"An error occurred while trying create a realm. {TechnicalMessage}"
        );

        public static Error UpdateRealmError => new(
            "Realm.UpdateRealmError",
            $"An error occurred while trying update the realm. {TechnicalMessage}"
        );

        public static Error DeleteRealmError => new(
            "Realm.DeleteRealmError",
            $"An error occurred while trying delete the realm. {TechnicalMessage}"
        );

        public static Error DisableRealmError => new(
            "Realm.DisableRealmError",
            $"An error occurred while trying disable/enable the realm. {TechnicalMessage}"
        );

        public static Error ReplicateRealmError => new(
            "Realm.ReplicateRealmError",
            $"An error occurred while trying replicate the realm. {TechnicalMessage}"
        );

        public static Error InvalidRealmNameError => new(
            "Realm.InvalidRealmNameError",
            $"Realm name is invalid. {TechnicalMessage}"
        );

        public static Error NotFoundError => new(
            "Realm.NotFoundError",
            $"Realm with provided name was not found. {TechnicalMessage}"
        );

        public static Error AllowedTenantsMapperNotFoundError => new(
            "Realm.AllowedTenantsMapperNotFoundError",
            $"Allowed tenants protocol mapper not found. {TechnicalMessage}"
        );

        public static Error UpdateClientScopeProtocolMapperError => new(
            "Realm.UpdateClientScopeProtocolMapperError",
            $"An error occurred while trying update the client scope protocol mapper. {TechnicalMessage}"
        );
    }
}
