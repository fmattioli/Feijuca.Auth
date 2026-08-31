using Feijuca.Auth.Errors;
using Feijuca.Auth.Http.BaseHttp;
using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Feijuca.Auth.Http.Client
{
    public class FeijucaAuthClient(HttpClient httpClient) : BaseHttpClient(httpClient), IFeijucaAuthClient
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly JsonSerializerOptions _serializer = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public async Task<Result<TokenDetailsResponse>> AuthenticateUserAsync(string tenant, string username, string password, CancellationToken cancellationToken)
        {
            IncludeTenantHeader(tenant);

            var result = await PostAsync<LoginUserRequest, TokenDetailsResponse>("users/login", new LoginUserRequest(username, password), cancellationToken);

            if (string.IsNullOrEmpty(result.AccessToken))
            {
                return Result<TokenDetailsResponse>.Failure(FeijucaErrors.GenerateTokenError);
            }

            return Result<TokenDetailsResponse>.Success(result);
        }

        public async Task<Result<UserResponse>> GetUserAsync(string tenant, string userame, string jwtToken, CancellationToken cancellationToken)
        {
            IncludeTenantHeader(tenant);

            var url = $"users?Usernames={userame}";
            var result = await GetAsync<PagedResult<UserResponse>>(url, jwtToken, cancellationToken);

            if (result.TotalResults == 0)
            {
                return Result<UserResponse>.Failure(FeijucaErrors.GetUserErrors);
            }

            var user = result?.Results.FirstOrDefault(x => x.Email == userame);

            return Result<UserResponse>.Success(user!);
        }

        public async Task<Result<PagedResult<UserResponse>>> GetUsersAsync(string tenant, int maxUsers, string jwtToken, CancellationToken cancellationToken)
        {
            IncludeTenantHeader(tenant);

            var url = $"users?PageFilter.Page=1&PageFilter.PageSize={maxUsers}";
            var result = await GetAsync<PagedResult<UserResponse>>(url, jwtToken, cancellationToken);

            if (result.TotalResults <= 1)
            {
                return Result<PagedResult<UserResponse>>.Failure(FeijucaErrors.GetUserErrors);
            }

            return Result<PagedResult<UserResponse>>.Success(result);
        }

        public async Task<Result<IEnumerable<GroupResponse>>> GetGroupsAsync(string tenant, string jwtToken, CancellationToken cancellationToken)
        {
            IncludeTenantHeader(tenant);

            var result = await GetAsync<IEnumerable<GroupResponse>>("groups", jwtToken, cancellationToken);

            if (!result.Any())
            {
                return Result<IEnumerable<GroupResponse>>.Failure(FeijucaErrors.GetGroupErrors);
            }

            return Result<IEnumerable<GroupResponse>>.Success(result);
        }

        public async Task<Result> RemoveUsersFromGroupAsync(RemoveUserFromGroupRequest removeUserFromGroupRequest, CancellationToken cancellationToken)
        {
            var result = await DeleteAsync<RemoveUserFromGroupRequest, bool>("groups/users", removeUserFromGroupRequest, cancellationToken);

            if (!result)
            {
                return Result.Failure(FeijucaErrors.RemoveUsersFromGroupError);
            }

            return Result.Success();
        }

        public async Task<Result<PagedResult<UserGroupResponse>>> GetGroupUsersAsync(string groupId, string jwtToken, CancellationToken cancellationToken)
        {
            var result = await GetAsync<PagedResult<UserGroupResponse>>($"groups/users?groupId={groupId}", jwtToken, cancellationToken);

            if (!result.Results.Any())
            {
                return Result<PagedResult<UserGroupResponse>>.Failure(FeijucaErrors.GetGroupUsersErrors);
            }

            return Result<PagedResult<UserGroupResponse>>.Success(result);
        }

        public async Task<Result<IEnumerable<RealmResponse>>> GetRealmsAsync(string jwtToken, CancellationToken cancellationToken)
        {
            var result = await GetAsync<IEnumerable<RealmResponse>>("realms", jwtToken, cancellationToken);

            if (!result.Any())
            {
                return Result<IEnumerable<RealmResponse>>.Failure(FeijucaErrors.GetGroupErrors);
            }

            return Result<IEnumerable<RealmResponse>>.Success(result);
        }

        public async Task<Result> CreateRealmAsync(IEnumerable<AddRealmRequest> AddRealmsRequest, CancellationToken cancellationToken)
        {
            var result = await _httpClient.PostAsJsonAsync("realms", AddRealmsRequest, cancellationToken);

            if (!result.IsSuccessStatusCode)
            {
                return Result.Failure(FeijucaErrors.CreateRealmError);
            }

            return Result.Success();
        }

        public async Task<Result> ReplicateRealmAsync(ReplicateRealmRequest request, CancellationToken cancellationToken)
        {
            var result = await PostAsync<ReplicateRealmRequest, bool>("realms/replicate", request, cancellationToken);

            if (!result)
            {
                return Result.Failure(FeijucaErrors.ReplicateRealmError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Creates a new group in the authentication service.
        /// </summary>
        /// <param name="request">Group creation data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Returns the created group identifier wrapped in a result object.
        /// </returns>
        public async Task<Result<string>> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync("groups", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode) 
            {
                var error = JsonSerializer.Deserialize<Error>(content, _serializer);
                return Result<string>.Failure(error ?? FeijucaErrors.CreateGroupError); 
            }

            return Result<string>.Success(content ?? "");
        }

        /// <summary>
        /// Creates a new user for the specified tenant.
        /// </summary>
        /// <param name="request">User creation data.</param>
        /// <param name="tenant">Tenant identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Returns the created user identifier wrapped in a result object.
        /// </returns>
        public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, string tenant, CancellationToken cancellationToken)
        {
            IncludeTenantHeader(tenant);

            var response = await _httpClient.PostAsJsonAsync("users", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<Error>(content, _serializer);
                return Result<Guid>.Failure(error ?? FeijucaErrors.CreateUserError);
            }

            var result = JsonSerializer.Deserialize<Guid>(content, _serializer);

            return Result<Guid>.Success(result);
        }

        /// <summary>
        /// Adds a user to a group in the authentication service.
        /// </summary>
        /// <param name="request">User and group association data.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// Returns a success result when the user is successfully added to the group.
        /// </returns>
        public async Task<Result> AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PostAsJsonAsync("groups/users", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<Error>(content, _serializer);
                return Result.Failure(error ?? FeijucaErrors.AddUserToGroupError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Update name from group.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Success Result when group name was updated sucessfully</returns>
        /// <returns>Failure Result when group name wasn't updated sucessfully</returns>
        public async Task<Result> UpdateGroupNameAsync(Guid groupId, UpdateGroupNameRequest request, CancellationToken cancellationToken)
        {
            var response = await _httpClient.PatchAsJsonAsync($"groups/{groupId}/update-group-name", request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<Error>(content, _serializer);
                return Result.Failure(error ?? FeijucaErrors.UpdateGroupNameError);
            }

            return Result.Success();
        }

        /// <summary>
        /// Sets the bearer token used in authenticated requests.
        /// </summary>
        /// <param name="jwtToken">JWT bearer token.</param>
        public void SetToken(string jwtToken)
        {
            if(!string.IsNullOrEmpty(jwtToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }
        }

        private void IncludeTenantHeader(string tenant)
        {
            if (!string.IsNullOrEmpty(tenant))
            {
                if (_httpClient.DefaultRequestHeaders.Contains("Tenant"))
                {
                    _httpClient.DefaultRequestHeaders.Remove("Tenant");
                }

                _httpClient.DefaultRequestHeaders.Add("Tenant", tenant);
            }
        }
    }
}
