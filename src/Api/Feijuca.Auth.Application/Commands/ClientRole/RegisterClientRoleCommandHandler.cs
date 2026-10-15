using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public class RegisterClientRoleCommandHandler(
    ITenantProvider tenantProvider,
    IClientRepository clientRepository,
    IClientRoleRepository clientRoleRepository,
    IRealmRepository realmRepository) : ICommandHandler<RegisterClientRoleCommand, Result>
{
    public async Task<Result> HandleAsync(RegisterClientRoleCommand request, CancellationToken cancellationToken = default)
    {
        var roles = request.Request.ClientRoles;
        if (!(roles?.Any() ?? false))
        {
            return Result.Failure(RoleErrors.AddRoleErrors);
        }

        var originTenant = tenantProvider.Tenant.Name;
        var originClientsResult = await clientRepository.GetClientsAsync(originTenant, cancellationToken);

        if (originClientsResult.IsFailure)
        {
            return Result.Failure(originClientsResult.Error);
        }

        var originClients = originClientsResult.Data;
        var failures = new List<string>();

        foreach (var role in roles)
        {
            var originClient = originClients.FirstOrDefault(c => c.ClientId == role.ClientId);
            if (originClient is null || !await EnsureRoleAsync(originClient.Id, role.Name, role.Description, originTenant, cancellationToken))
            {
                failures.Add($"{originTenant}/{role.Name}");
            }
        }

        var realms = await realmRepository.GetAllAsync(cancellationToken);
        var targetTenants = realms.Select(r => r.Realm).Where(r => r != originTenant);

        foreach (var targetTenant in targetTenants)
        {
            var targetClientsResult = await clientRepository.GetClientsAsync(targetTenant, cancellationToken);
            if (targetClientsResult.IsFailure)
            {
                failures.AddRange(roles.Select(role => $"{targetTenant}/{role.Name}"));
                continue;
            }

            foreach (var role in roles)
            {
                var targetClient = targetClientsResult.Data.FirstOrDefault(c => c.ClientId == role.ClientId);
                if (targetClient is null || !await EnsureRoleAsync(targetClient.Id, role.Name, role.Description, targetTenant, cancellationToken))
                {
                    failures.Add($"{targetTenant}/{role.Name}");
                }
            }
        }

        if (failures.Count > 0)
        {
            return Result.Failure(new Error("Role.PropagationFailed", $"Failed to propagate role(s) to: {string.Join(", ", failures)}"));
        }

        return Result.Success();
    }

    private async Task<bool> EnsureRoleAsync(string clientId, string roleName, string description, string tenant, CancellationToken cancellationToken)
    {
        var existingRoles = await clientRoleRepository.GetRolesForClientAsync(clientId, tenant, cancellationToken);
        if (existingRoles.IsSuccess && (existingRoles.Data?.Any(r => r.Name == roleName) ?? false))
        {
            return true;
        }

        var addResult = await clientRoleRepository.AddClientRoleAsync(clientId, roleName, description, tenant, cancellationToken);

        return addResult.IsSuccess;
    }
}
