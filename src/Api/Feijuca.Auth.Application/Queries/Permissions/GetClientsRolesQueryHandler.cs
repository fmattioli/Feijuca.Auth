using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Providers;
using LiteBus.Queries.Abstractions;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Application.Queries.Permissions
{
    public class GetClientsRolesQueryHandler(IClientRepository clientRepository, 
        IClientRoleRepository roleRepository,
        ITenantProvider tenantProvider) : IQueryHandler<GetClientRolesQuery, Result<IEnumerable<ClientRoleResponse>>>
    {
        private readonly IClientRepository _clientRepository = clientRepository;
        private readonly IClientRoleRepository _roleRepository = roleRepository;

        public async Task<Result<IEnumerable<ClientRoleResponse>>> HandleAsync(GetClientRolesQuery request, CancellationToken cancellationToken = default)
        {
            var result = await _clientRepository.GetClientsAsync(tenantProvider.GetRequestedTenant()!.Name, cancellationToken);
            if (result.IsSuccess)
            {
                var roleResponse = new List<ClientRoleResponse>();
                foreach (var client in result.Data)
                {
                    var rolesResult = await _roleRepository.GetRolesForClientAsync(client.Id, tenantProvider.GetRequestedTenant()!.Name, cancellationToken);

                    if (rolesResult.IsSuccess)
                    {
                        var rolesResponse = rolesResult.Data.Select(x => new RoleResponse(x.Id, x.Name, x.Description ?? "", false, false, string.Empty));
                        roleResponse.Add(new ClientRoleResponse(client.ClientId, client.Id, rolesResponse));
                    }
                }

                return Result<IEnumerable<ClientRoleResponse>>.Success(roleResponse);
            }

            return Result<IEnumerable<ClientRoleResponse>>.Failure(RoleErrors.GetRoleErrors);
        }
    }
}
