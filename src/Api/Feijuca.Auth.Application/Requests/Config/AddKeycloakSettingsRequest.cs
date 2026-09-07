using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Application.Requests.Config
{
    public record AddKeycloakSettingsRequest(LoginUserRequest RealmAdminUser, Models.Client Client, Secrets ClientSecret, ServerSettings ServerSettings, Models.Realm Realm);
}
