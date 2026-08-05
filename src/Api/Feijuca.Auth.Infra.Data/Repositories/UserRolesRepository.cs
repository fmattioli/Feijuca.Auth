using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using Flurl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace Feijuca.Auth.Infra.Data.Repositories;

public class UserRolesRepository(IHttpClientFactory httpClientFactory, IAuthRepository _authRepository, ITenantProvider _tenantService)
    : BaseRepository(httpClientFactory), IUserRolesRepository
{
    public async Task<Result<IEnumerable<ClientMapping>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken)
    {
        var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
        using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

        var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(userId)
                .AppendPathSegment("role-mappings");

        using var response = await httpClient.GetAsync(url, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var userRolesContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonObject = JObject.Parse(userRolesContent);
            var clientMappings = jsonObject["clientMappings"] is null ? [] :
                jsonObject["clientMappings"]?
                    .Children<JProperty>()
                    .Select(x => x.Value.ToObject<ClientMapping>()!)
                    .ToList();

            return Result<IEnumerable<ClientMapping>>.Success(clientMappings!);
        }

        return Result<IEnumerable<ClientMapping>>.Failure(UserRolesErrors.ErrorGetUserRoles);
    }

    public async Task<Result> AddClientRoleToUserAsync(string userId, string clientId, Guid roleId, string roleName, CancellationToken cancellationToken)
    {
        var tokenDetails = await _authRepository.GetAccessTokenAsync(cancellationToken);
        using var httpClient = CreateHttpClientWithHeaders(tokenDetails.Data.Access_Token);

        var url = httpClient.BaseAddress
                .AppendPathSegment("admin")
                .AppendPathSegment("realms")
                .AppendPathSegment(_tenantService.Tenant.Name)
                .AppendPathSegment("users")
                .AppendPathSegment(userId)
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
            return Result.Success();
        }

        return Result.Failure(UserRolesErrors.ErrorAddRoleToUser);
    }
}
