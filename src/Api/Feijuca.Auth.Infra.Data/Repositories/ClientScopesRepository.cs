using Feijuca.Auth.Common;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Flurl;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace Feijuca.Auth.Infra.Data.Repositories
{
    public class ClientScopesRepository(IHttpClientFactory httpClientFactory, IAuthRepository authRepository)
        : BaseRepository(httpClientFactory), IClientScopesRepository
    {
        public async Task<bool> AddAudienceMapperAsync(string clientScopeId, string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes")
                .AppendPathSegment($"{clientScopeId}")
                .AppendPathSegment($"protocol-mappers")
                .AppendPathSegment($"models");

            var audienceMapper = new
            {
                name = Constants.FeijucaApiClientName,
                protocol = "openid-connect",
                protocolMapper = "oidc-audience-mapper",
                config = new Dictionary<string, string>
                {
                    { "included.client.audience", Constants.FeijucaApiClientName },
                    { "id.token.claim", "true" },
                    { "access.token.claim", "true" },
                    { "claim.name", "aud" },
                    { "userinfo.token.claim", "false" },
                    { "access.token.introspection", "true" },
                    { "lightweight.access.token.claim", "false" }
                }
            };

            var response = await httpClient.PostAsJsonAsync(url, audienceMapper, cancellationToken: cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddGroupMembershipMapperAsync(string clientScopeId, string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);

            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes")
                .AppendPathSegment(clientScopeId)
                .AppendPathSegment("protocol-mappers")
                .AppendPathSegment("models");

            var groupMembershipMapper = new
            {
                name = "groups",
                protocol = "openid-connect",
                protocolMapper = "oidc-group-membership-mapper",
                config = new Dictionary<string, string>
                {
                    { "claim.name", "groups" },

                    { "access.token.claim", "true" },
                    { "id.token.claim", "true" },
                    { "userinfo.token.claim", "false" },

                    { "full.path", "false" }
                }
            };

            using var response = await httpClient.PostAsJsonAsync(
                url,
                groupMembershipMapper,
                cancellationToken);

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> AddUserPropertyMapperAsync(string clientScopeId, 
            string userPropertyName, 
            string claimName, 
            string tenant,
            CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes")
                .AppendPathSegment(clientScopeId)
                .AppendPathSegment("protocol-mappers")
                .AppendPathSegment("models");

            var userPropertyMapper = new
            {
                name = $"{claimName}-mapper",
                protocol = "openid-connect",
                protocolMapper = "oidc-usermodel-attribute-mapper", //when wish create property type instead of attribute type use oidc-usermodel-property-mapper
                consentRequired = false,
                config = new Dictionary<string, string>
                {
                    { "user.attribute", userPropertyName },
                    { "claim.name", claimName },
                    { "jsonType.label", "String" },
                    { "id.token.claim", "true" },
                    { "access.token.claim", "true" },
                    { "userinfo.token.claim", "true" }
                }
            };

            var response = await httpClient.PostAsJsonAsync(url, userPropertyMapper, cancellationToken);

            return response.IsSuccessStatusCode;
        }

        public async Task<string?> AddClientScopesAsync(ClientScopeEntity clientScopesEntity, string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes");

            var clientScope = new
            {
                name = clientScopesEntity.Name,
                description = clientScopesEntity.Description,
                protocol = "openid-connect",
                attributes = new Dictionary<string, bool>
                {
                    { "display.on.consent.screen", true },
                    { "include.in.token.scope", true }
                },
                defaultScope = true
            };

            var jsonContent = JsonConvert.SerializeObject(clientScope);
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            var location = response.Headers.Location?.ToString();
            if (string.IsNullOrWhiteSpace(location))
                return null;

            return location.Split('/').Last();
        }


        public async Task<bool> AddClientScopeToClientAsync(string clientId,
            string tenant,
            string clientScopeId,
            bool isOptional,
            CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("clients")
                .AppendPathSegment(clientId)
                .AppendPathSegment(isOptional ? "optional-client-scopes" : "default-client-scopes")
                .AppendPathSegment(clientScopeId);

            using var response = await httpClient.PutAsync(url, default, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        public async Task<IEnumerable<ClientScopeEntity>> GetClientScopesAsync(string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes");

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return [];

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var clientScopes = JsonConvert.DeserializeObject<IEnumerable<ClientScopeEntity>>(responseBody)
                              ?? [];

            var defaultClientScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "email",
                "roles",
                "web-origins",
                "acr",
                "offline_access",
                "microprofile-jwt",
                "phone",
                "role_list",
                "address",
                "acr"
            };

            return clientScopes.Where(scope => !defaultClientScopes.Contains(scope?.Name ?? ""));
        }

        public async Task<IEnumerable<ClientScopeEntity>> GetClientScopesAssociatedToTheClientAsync(string tenant, string clientId, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            async Task<IEnumerable<ClientScopeEntity>> GetAsync(params string[] segments)
            {
                var url = httpClient.BaseAddress!.ToString();
                foreach (var segment in segments)
                {
                    url = url.AppendPathSegment(segment);
                }

                using var response = await httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return [];

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonConvert.DeserializeObject<IEnumerable<ClientScopeEntity>>(body) ?? [];
            }

            var defaultScopes = await GetAsync(
                "admin", "realms", tenant, "clients", clientId, "default-client-scopes");

            var optionalScopes = await GetAsync(
                "admin", "realms", tenant, "clients", clientId, "optional-client-scopes");


            var defaultClientScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "profile",
                "email",
                "roles",
                "web-origins",
                "acr",
                "offline_access",
                "microprofile-jwt",
                "phone",
                "role_list",
                "address",
                "acr"
            };

            return defaultScopes
                .Concat(optionalScopes)
                .GroupBy(s => s.Id)
                .Select(g => g.First())
                .Where(scope => !defaultClientScopes.Contains(scope?.Name ?? ""));
        }


        public async Task<ClientScopeEntity> GetClientScopeProfileAsync(string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("client-scopes");

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return null!;
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var clientScopes = JsonConvert.DeserializeObject<IEnumerable<ClientScopeEntity>>(responseBody) ?? [];

            var defaultClientScopes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "profile"
            };

            return clientScopes.First(scope => string.Equals(scope?.Name, "profile", StringComparison.OrdinalIgnoreCase));
        }
    }
}
