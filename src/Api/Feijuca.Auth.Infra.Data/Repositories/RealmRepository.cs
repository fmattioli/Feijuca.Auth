using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Json;
using System.Text;

namespace Feijuca.Auth.Infra.Data.Repositories
{
    public class RealmRepository(IHttpClientFactory httpClientFactory, IAuthRepository authRepository) : BaseRepository(httpClientFactory), IRealmRepository
    {
        private readonly IAuthRepository _authRepository = authRepository;

        private static readonly JsonSerializerSettings _settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };

        public async Task<IEnumerable<RealmEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                   .AppendPathSegment("admin")
                   .AppendPathSegment("realms");

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var keycloakRealmContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var realms = JsonConvert.DeserializeObject<IEnumerable<RealmEntity>>(keycloakRealmContent)!;

            return realms.Where(x => x.Realm != "master") ?? [];
        }

        public async Task<Result<RealmEntity>> GetAsync(string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(name) || name == "master")
            {
                return Result<RealmEntity>.Failure(RealmErrors.InvalidRealmNameError);
            }

            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(name);

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var keycloakRealmContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var realm = JsonConvert.DeserializeObject<RealmEntity>(keycloakRealmContent)!;

            if (realm is null)
            {
                return Result<RealmEntity>.Failure(RealmErrors.NotFoundError);
            }

            return Result<RealmEntity>.Success(realm);
        }

        public async Task<bool> CreateRealmAsync(RealmEntity realm, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                   .AppendPathSegment("admin")
                   .AppendPathSegment("realms");

            var realmBody = new
            {
                realm = realm.Realm,
                enabled = realm.Enabled,
                displayName = realm.DisplayName,
                accessTokenLifespan = realm.AccessTokenLifespan,
                accessTokenLifespanForImplicitFlow = realm.AccessTokenLifespanForImplicitFlow,
                ssoSessionIdleTimeout = realm.SsoSessionIdleTimeout,
                ssoSessionMaxLifespan = realm.SsoSessionMaxLifespan,
                offlineSessionIdleTimeout = realm.OfflineSessionIdleTimeout,
                offlineSessionMaxLifespan = realm.OfflineSessionMaxLifespan
            };

            var jsonContent = JsonConvert.SerializeObject(realmBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var resut = await httpClient.PostAsync(url, content, cancellationToken);
            if (resut.IsSuccessStatusCode)
            {
                return await UpdateRealmUnmanagedAttributePolicyAsync(realm.Realm, cancellationToken);
            }

            return false;
        }

        public async Task<Result> UpdateRealmAsync(string name, RealmEntity realm, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(name);

            var json = JsonConvert.SerializeObject(realm, _settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            RealmErrors.SetTechnicalMessage(responseMessage);
            return Result.Failure(RealmErrors.UpdateRealmError);
        }

        public async Task<bool> DeleteRealmAsync(string name, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(name);

            using var response = await httpClient.DeleteAsync(url, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EnableDisableRealmAsync(string name, bool enable, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(name);

            var payload = new
            {
                enabled = enable
            };

            using var response = await httpClient.PutAsJsonAsync(url, payload, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<string> GetRealmConfigAsync(string name, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(name)
                    .AppendPathSegment("clients");

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<string>(responseContent)!;
                return result;
            }

            return "";
        }

        public async Task<bool> UpdateRealmUnmanagedAttributePolicyAsync(string realmName, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                       .AppendPathSegment("admin")
                       .AppendPathSegment("realms")
                       .AppendPathSegment(realmName)
                       .AppendPathSegment("users")
                       .AppendPathSegment("profile");

            var responseGet = await httpClient.GetAsync(url, cancellationToken);
            if (!responseGet.IsSuccessStatusCode)
                return false;

            var jsonCurrentProfile = await responseGet.Content.ReadAsStringAsync(cancellationToken);

            dynamic profile = JsonConvert.DeserializeObject<dynamic>(jsonCurrentProfile)!;

            profile.unmanagedAttributePolicy = "ENABLED";

            var jsonUpdatedProfile = JsonConvert.SerializeObject(profile);

            var content = new StringContent(jsonUpdatedProfile, Encoding.UTF8, "application/json");

            var responsePut = await httpClient.PutAsync(url, content, cancellationToken);

            return responsePut.IsSuccessStatusCode;
        }
    }
}