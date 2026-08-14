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
    public class GroupRepository(IHttpClientFactory httpClientFactory, IAuthRepository _authRepository, ITenantProvider _tenantProvider) 
        : BaseRepository(httpClientFactory), IGroupRepository
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
        };

        public async Task<Result<IEnumerable<Group>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var tokenDetailsResult = await _authRepository.GetAccessTokenAsync(cancellationToken);

            if (tokenDetailsResult.IsSuccess)
            {
                using var httpClient = CreateHttpClientWithHeaders(tokenDetailsResult.Data.Access_Token);

                var url = httpClient.BaseAddress
                        .AppendPathSegment("admin")
                        .AppendPathSegment("realms")
                        .AppendPathSegment(_tenantProvider.Tenant.Name)
                        .AppendPathSegment("groups");

                using var response = await httpClient.GetAsync(url, cancellationToken);
                var groups = await response.Content.ReadAsStringAsync(cancellationToken);
                var users = JsonConvert.DeserializeObject<IEnumerable<Group>>(groups)!;

                return Result<IEnumerable<Group>>.Success(users);
            }

            return Result<IEnumerable<Group>>.Failure(tokenDetailsResult.Error);
        }

        public async Task<Result<IEnumerable<Group>>> GetGroupByNameAsync(
            string? groupName,
            string? tenant,
            CancellationToken cancellationToken)
        {
            var tokenDetailsResult = await _authRepository.GetAccessTokenAsync(cancellationToken);

            if (!tokenDetailsResult.IsSuccess)
                return Result<IEnumerable<Group>>.Failure(tokenDetailsResult.Error);

            using var httpClient = CreateHttpClientWithHeaders(tokenDetailsResult.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant ?? _tenantProvider.Tenant.Name)
                .AppendPathSegment("groups");

            if (!string.IsNullOrWhiteSpace(groupName))
            {
                url = url.SetQueryParam("search", groupName);
            }

            using var response = await httpClient.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            var groupsJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var groups = JsonConvert.DeserializeObject<IEnumerable<Group>>(groupsJson)!;

            return Result<IEnumerable<Group>>.Success(groups);
        }

        public async Task<Result<string>> CreateAsync(
            string name,
            string tenant,
            Dictionary<string, string[]> attributes,
            CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(tenant)
                .AppendPathSegment("groups");

            var group = new
            {
                name,
                attributes
            };

            var jsonContent = JsonConvert.SerializeObject(group);
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                GroupErrors.SetTechnicalMessage(response.ReasonPhrase!);
                return Result<string>.Failure(GroupErrors.CreationGroupError);
            }

            var location = response.Headers.Location?.ToString();

            if (string.IsNullOrWhiteSpace(location))
            {
                GroupErrors.SetTechnicalMessage("Header Location não retornado pelo Keycloak.");
                return Result<string>.Failure(GroupErrors.CreationGroupError);
            }

            var groupId = location.Split('/').Last();

            return Result<string>.Success(groupId);
        }

        public async Task<Result> UpdateAsync(Group group, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);

            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantProvider.Tenant.Name)
                .AppendPathSegment("groups")
                .AppendPathSegment(group.Id);

            var jsonContent = JsonConvert.SerializeObject(group, Settings);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            using var response = await httpClient.PutAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken);
            GroupErrors.SetTechnicalMessage(responseMessage);
            return Result<bool>.Failure(GroupErrors.UpdateGroupError);
        }

        public async Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantProvider.Tenant.Name)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(id);

            using var response = await httpClient.DeleteAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            GroupErrors.SetTechnicalMessage(response.ReasonPhrase!);
            return Result.Failure(GroupErrors.DeletionGroupError);
        }

        public async Task<Result<IEnumerable<Domain.Entities.User>>> GetUsersInGroupAsync(string id, UserFilters userFilters, int totalUsers, CancellationToken cancellationToken)
        {            
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);
            int first = (userFilters.PageFilter.PageNumber - 1) * totalUsers;

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantProvider.Tenant.Name)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(id)
                    .AppendPathSegment("members")
                    .SetQueryParam("first", first)
                    .SetQueryParam("max", totalUsers);

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var users = JsonConvert.DeserializeObject<IEnumerable<Domain.Entities.User>>(responseContent);

                return Result<IEnumerable<Domain.Entities.User>>.Success(users!);
            }

            return Result<IEnumerable<Domain.Entities.User>>.Failure(GroupErrors.GetUsersInGroupsError);
        }
    }
}
