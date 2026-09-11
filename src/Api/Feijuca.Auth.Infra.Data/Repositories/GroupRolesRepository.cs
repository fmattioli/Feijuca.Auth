using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Providers;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;

namespace Feijuca.Auth.Infra.Data.Repositories
{
    public class GroupRolesRepository(IHttpClientFactory httpClientFactory, IAuthRepository _authRepository, ITenantProvider _tenantService)
        : BaseRepository(httpClientFactory), IGroupRolesRepository
    {

        public async Task<Result<bool>> AddClientRoleToGroupAsync(string groupId,
            string clientId, 
            Guid roleId, 
            string roleName, 
            string tenant, 
            CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(tenant)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(groupId)
                    .AppendPathSegment("role-mappings")
                    .AppendPathSegment("clients")
                    .AppendPathSegment(clientId);

            var roleData = new[]
            {
                new { id = roleId, name = roleName }
            };

            var content = new StringContent(JsonConvert.SerializeObject(roleData), Encoding.UTF8, "application/json");

            using var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            var groupRolesContent = await response.Content.ReadAsStringAsync(cancellationToken);

            return Result<bool>.Failure(GroupRolesErrors.ErrorAddRoleToGroup);
        }

        public async Task<Result<IEnumerable<ClientMapping>>> GetGroupRolesAsync(string groupId, string tenant, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(tenant)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(groupId)
                    .AppendPathSegment("role-mappings");

            using var response = await httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var groupRolesContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var jsonObject = JObject.Parse(groupRolesContent);
                var clientMappings = jsonObject["clientMappings"] is null ? [] : 
                    jsonObject["clientMappings"]?
                        .Children<JProperty>()
                        .Select(x => x.Value.ToObject<ClientMapping>()!)
                        .ToList();

                return Result<IEnumerable<ClientMapping>>.Success(clientMappings!);
            }

            return Result<IEnumerable<ClientMapping>>.Failure(GroupRolesErrors.ErrorGetGroupRoles);
        }

        public async Task<Result> RemoveRoleFromGroupAsync(string clientId, string groupId, Guid roleId, string roleName, CancellationToken cancellationToken)
        {
            var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
            using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

            var url = httpClient.BaseAddress
                    .AppendPathSegment("admin")
                    .AppendPathSegment("realms")
                    .AppendPathSegment(_tenantService.Tenant.Name)
                    .AppendPathSegment("groups")
                    .AppendPathSegment(groupId)
                    .AppendPathSegment("role-mappings")
                    .AppendPathSegment("clients")
                    .AppendPathSegment(clientId);

            var body = JsonConvert.SerializeObject(new[]
            {
                new {
                    id = roleId,
                    name = roleName
                }
            });

            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(GroupRolesErrors.RemovingRoleFromGroupError);
        }
    }
}
