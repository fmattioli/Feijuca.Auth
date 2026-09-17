using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using Flurl;
using Newtonsoft.Json;

namespace Feijuca.Auth.Infra.Data.Repositories
{
    public class UserGroupRepository(IHttpClientFactory httpClientFactory, 
        IAuthRepository _authRepository, 
        ITenantProvider _tenantService) : BaseRepository(httpClientFactory), IGroupUsersRepository
    {
        public async Task<Result<bool>> AddUserToGroupAsync(Guid userId, string tenant, Guid groupId, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(tenant)
                    .AppendPathSegment("users")
                    .AppendPathSegment(userId)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(groupId);

            var response = await httpClient.PutAsync(url, default, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserGroupErrors.ErrorAddUserToGroup);
        }

        public async Task<Result<bool>> RemoveUserFromGroupAsync(Guid userId, Guid groupId, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users")
                    .AppendPathSegment(userId)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(groupId);

            using var response = await httpClient.DeleteAsync(url, default);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserGroupErrors.ErrorAddUserToGroup);
        }

        public async Task<Result<IEnumerable<Group>>> GetUserGroupsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("users")
                    .AppendPathSegment(userId)
                    .AppendPathSegment("groups");

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if(!response.IsSuccessStatusCode)
            {
                UserGroupErrors.SetTechnicalMessage(await response.Content.ReadAsStringAsync(cancellationToken));
                return Result<IEnumerable<Group>>.Failure(UserGroupErrors.ErrorGetGroup);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var groups = JsonConvert.DeserializeObject<IEnumerable<Group>>(content);

            return Result<IEnumerable<Group>>.Success(groups ?? []);
        }
    }
}