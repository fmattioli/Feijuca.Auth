using Feijuca.Auth.Application.Requests.Role;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public class AddClientRoleCommandHandler(IClientRoleRepository clientRolesRepository,
    IRealmRepository realmRepository,
    ITenantProvider tenantProvider,
    ILogger<AddClientRoleCommandHandler> logger) : ICommandHandler<AddClientRoleCommand, Result<bool>>
{
    private readonly IClientRoleRepository _roleRepository = clientRolesRepository;

    public async Task<Result<bool>> HandleAsync(AddClientRoleCommand request, CancellationToken cancellationToken)
    {
        if (!(request.AddClientRolesRequest.ClientRoles?.Any() ?? false))
        {
            logger.LogWarning("No client roles provided in the request.");
            return Result<bool>.Failure(RoleErrors.AddRoleErrors);
        }

        var result = await AddClientRoleAsync(request.AddClientRolesRequest.ClientRoles, tenantProvider.Tenant.Name, cancellationToken);

        if (request.AddClientRolesRequest.AllTenants)
        {
            var realms = await realmRepository.GetAllAsync(cancellationToken);
            var tenants = realms.Where(r => r.Realm != tenantProvider.Tenant.Name).Select(r => r.Realm);

            foreach (var tenant in tenants)
            {
                var results = await AddClientRoleAsync(request.AddClientRolesRequest.ClientRoles, tenant, cancellationToken);
                if (!results.IsSuccess)
                {
                    logger.LogError("Failed to add client roles for tenant {Tenant}.", tenant);
                    return Result<bool>.Failure(RoleErrors.AddRoleErrors);
                }

                logger.LogInformation("Successfully added client roles for tenant {Tenant}.", tenant);
            }

            return Result<bool>.Success(true);
        }

        return result;
    }

    private async Task<Result<bool>> AddClientRoleAsync(IEnumerable<AddClientRoleRequest> clientRoles, string tenant, CancellationToken cancellationToken)
    {
        foreach (var clientRole in clientRoles)
        {
            var result = await _roleRepository.AddClientRoleAsync(
                clientRole.ClientId,
                clientRole.Name,
                clientRole.Description,
                tenant,
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Result<bool>.Failure(RoleErrors.AddRoleErrors);
            }
        }

        return Result<bool>.Success(true);
    }

}