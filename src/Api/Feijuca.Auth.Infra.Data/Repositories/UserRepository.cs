using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Filters;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Providers;
using Flurl;
using Feijuca.Auth.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace Feijuca.Auth.Infra.Data.Repositories
{
    public class UserRepository(IClientRepository clientRepository,
        IHttpClientFactory httpClientFactory,
        IAuthRepository authRepository,
        ITenantProvider tenantService) : BaseRepository(httpClientFactory), IUserRepository
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly IAuthRepository _authRepository = authRepository;
        private readonly ITenantProvider _tenantService = tenantService;

        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };

        public async Task<Result<IEnumerable<Domain.Entities.User>>> GetAllAsync(UserFilters userFilters, int totalUsers, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            int first = (userFilters.PageFilter.PageNumber - 1) * userFilters.PageFilter.PageSize;

            var urlGetUsers = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .SetQueryParam("first", first)
                .SetQueryParam("max", totalUsers);

            using var response = await httpClient.GetAsync(urlGetUsers, cancellationToken);
            var keycloakUserContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var users = JsonConvert.DeserializeObject<IEnumerable<Domain.Entities.User>>(keycloakUserContent)!;

            return Result<IEnumerable<Domain.Entities.User>>.Success(users
                .OrderBy(a => a.FirstName)
                .ThenBy(a => a.LastName));
        }

        public async Task<int> GetTotalAsync(CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var urlGetUsers = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .SetQueryParam("first", 0)
                .SetQueryParam("max", 99999);

            using var response = await httpClient.GetAsync(urlGetUsers, cancellationToken);
            var keycloakUserContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var users = JsonConvert.DeserializeObject<IEnumerable<Domain.Entities.User>>(keycloakUserContent)!;

            return users.Count();
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(id);

            using var response = await httpClient.DeleteAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            UserErrors.SetTechnicalMessage(response.ReasonPhrase!);
            return Result<bool>.Failure(UserErrors.DeletionUserError);
        }

        public async Task<Result<string>> CreateAsync(Domain.Entities.User user, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var tenant = user.Attributes["Tenant"]?.FirstOrDefault();
            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("users");

            var json = JsonConvert.SerializeObject(user, Settings);
            using var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(url, httpContent, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                UserErrors.SetTechnicalMessage(error);
                return Result<string>.Failure(UserErrors.UserCreationError);
            }

            var location = response.Headers.Location?.ToString();

            if (string.IsNullOrWhiteSpace(location))
            {
                UserErrors.SetTechnicalMessage("Header Location not found.");
                return Result<string>.Failure(UserErrors.UserCreationError);
            }

            var userId = location.Split('/').Last();

            return Result<string>.Success(userId);
        }


        public async Task<Result<Domain.Entities.User>> GetAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Result<Domain.Entities.User>.Failure(UserErrors.InvalidUserNameOrPasswordError);
            }

            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users");

            url = url.SetQueryParam("username", username);

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var keycloakUserContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var user = JsonConvert.DeserializeObject<List<Domain.Entities.User>>(keycloakUserContent)!;

            if (user.Count == 0)
            {
                return Result<Domain.Entities.User>.Failure(UserErrors.InvalidUserNameOrPasswordError);
            }

            return Result<Domain.Entities.User>.Success(user[0]);
        }

        public async Task<Result<Domain.Entities.User>> GetAsync(string username, string tenant, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(username))
            {
                return Result<Domain.Entities.User>.Failure(UserErrors.InvalidUserNameOrPasswordError);
            }

            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(tenant)
                    .AppendPathSegment("users");

            url = url.SetQueryParam("username", username);

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var keycloakUserContent = await response.Content.ReadAsStringAsync(cancellationToken);

            var user = JsonConvert.DeserializeObject<List<Domain.Entities.User>>(keycloakUserContent)!;

            if (user.Count == 0)
            {
                return Result<Domain.Entities.User>.Failure(UserErrors.InvalidUserNameOrPasswordError);
            }

            return Result<Domain.Entities.User>.Success(user[0]);
        }

        public async Task<Result<Domain.Entities.User>> GetByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return Result<Domain.Entities.User>.Failure(UserErrors.InvalidId);
            }

            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users")
                    .AppendPathSegment(userId);

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var keycloakUserContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var user = JsonConvert.DeserializeObject<Domain.Entities.User>(keycloakUserContent)!;
                return Result<Domain.Entities.User>.Success(user);
            }

            return Result<Domain.Entities.User>.Failure(UserErrors.InvalidId);
        }

        public async Task<Result<bool>> ResetPasswordAsync(Guid id, string password, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users")
                    .AppendPathSegment(id)
                    .AppendPathSegment("reset-password");

            var passwordData = new
            {
                type = "password",
                temporary = false,
                value = password
            };

            var json = JsonConvert.SerializeObject(passwordData, Settings);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, httpContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result<bool>.Failure(UserErrors.WrongPasswordDefinition);
        }

        public async Task<Result<bool>> ResetPasswordAsync(Guid id, string password, string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(tenant)
                    .AppendPathSegment("users")
                    .AppendPathSegment(id)
                    .AppendPathSegment("reset-password");

            var passwordData = new
            {
                type = "password",
                temporary = false,
                value = password
            };

            var json = JsonConvert.SerializeObject(passwordData, Settings);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, httpContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result<bool>.Failure(UserErrors.WrongPasswordDefinition);
        }

        public async Task<Result> SendEmailVerificationAsync(string userId, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress!
                .ToString()
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(userId);

            var requestData = new
            {
                requiredActions = new string[] { "VERIFY_EMAIL" }
            };

            var json = JsonConvert.SerializeObject(requestData, Settings);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, httpContent, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                url = url.AppendPathSegment("send-verify-email");
                await httpClient.PutAsync(url, default!, cancellationToken);
                return Result.Success();
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result.Failure(UserErrors.InvalidUserNameOrPasswordError);
        }

        public async Task<Result<bool>> RevokeSessionsByUserIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress!
                .ToString()
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(id)
                .AppendPathSegment("logout");

            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            using var resut = await httpClient.PostAsync(url, content, cancellationToken);

            if (resut.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserErrors.RevokeSessionsError);
        }

        public async Task<Result<bool>> RevokeSessionAsync(string sessionId, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress!
                .ToString()
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("sessions")
                .AppendPathSegment(sessionId);

            using var result = await httpClient.DeleteAsync(url, cancellationToken);

            if (result.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserErrors.RevokeSessionsError);
        }


        public async Task<Result<TokenDetails>> LoginAsync(string username, string password, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("KeycloakClient");

            var urlGetToken = httpClient.BaseAddress
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("protocol")
                .AppendPathSegment("openid-connect")
                .AppendPathSegment("token");

            var client = await clientRepository.GetClientAsync("feijuca-auth-api", _tenantService.Tenant.Name, cancellationToken);

            var requestData = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", client.Data.ClientId),
                new KeyValuePair<string, string>("client_secret", client.Data.Secret),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            ]);

            var response = await httpClient.PostAsync(urlGetToken, requestData, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var userResult = await GetAsync(username, cancellationToken);

                if (userResult.IsFailure)
                {
                    return Result<TokenDetails>.Failure(UserErrors.InvalidUserNameOrPasswordError);
                }

                var userId = userResult.Data.Id;

                var userSessionsResult = await GetUserSessionsAsync(userId, cancellationToken);

                if (userSessionsResult.IsSuccess && userSessionsResult.Data.Any())
                {
                    var sessionsToRevoke = userSessionsResult.Data
                        .OrderByDescending(s => s.Start)
                        .Skip(1)
                        .ToList();

                    foreach (var session in sessionsToRevoke)
                    {
                        await RevokeSessionAsync(session.Id!, cancellationToken);
                    }
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<TokenDetails>(content);

                return Result<TokenDetails>.Success(result!);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result<TokenDetails>.Failure(UserErrors.InvalidUserNameOrPasswordError);
        }

        public async Task<Result<bool>> SignoutAsync(string refreshToken, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("KeycloakClient");

            var urlGetToken = httpClient.BaseAddress
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("protocol")
                .AppendPathSegment("openid-connect")
                .AppendPathSegment("logout");

            var client = await clientRepository.GetClientAsync("feijuca-auth-api", _tenantService.Tenant.Name, cancellationToken);

            var requestData = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("client_id", client.Data.ClientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
                new KeyValuePair<string, string>("client_secret", client.Data.Secret)
            ]);

            var response = await httpClient.PostAsync(urlGetToken, requestData, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserErrors.SignOutError);
        }

        public async Task<Result<TokenDetails>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            using var httpClient = _httpClientFactory.CreateClient("KeycloakClient");

            var urlGetToken = httpClient.BaseAddress.AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("protocol")
                .AppendPathSegment("openid-connect")
                .AppendPathSegment("token");

            var client = await clientRepository.GetClientAsync("feijuca-auth-api", _tenantService.Tenant.Name, cancellationToken);

            var requestData = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", client.Data.ClientId),
                new KeyValuePair<string, string>("client_secret", client.Data.Secret),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            ]);

            var response = await httpClient.PostAsync(urlGetToken, requestData, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonConvert.DeserializeObject<TokenDetails>(content);
                return Result<TokenDetails>.Success(result!);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result<TokenDetails>.Failure(UserErrors.InvalidRefreshToken);
        }

        public async Task<Result<bool>> UpdateUserAsync(Guid id, Domain.Entities.User user, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);

            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users")
                    .AppendPathSegment(id);

            var json = JsonConvert.SerializeObject(user, Settings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            UserErrors.SetTechnicalMessage(responseMessage);
            return Result<bool>.Failure(UserErrors.InvalidRefreshToken);
        }

        public async Task<Result<IEnumerable<KeycloakSession>>> GetUserSessionsAsync(Guid id, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(id)
                .AppendPathSegment("sessions");

            using var response = await httpClient.GetAsync(url, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                UserErrors.SetTechnicalMessage(content);
                return Result<IEnumerable<KeycloakSession>>.Failure(UserErrors.GetUserSessionsError);
            }

            var sessions = JsonConvert.DeserializeObject<IEnumerable<KeycloakSession>>(content)!;
            return Result<IEnumerable<KeycloakSession>>.Success(sessions);
        }
    }
}
