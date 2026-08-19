using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public class SyncClientRoleCommandHandler(ITenantProvider tenantProvider,
    IClientRepository clientRepository,
    IClientRoleRepository clientRoleRepository) : ICommandHandler<SyncClientRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(SyncClientRoleCommand request, CancellationToken cancellationToken = default)
    {
        var originTenant = tenantProvider.Tenant.Name;
        var originClients = await clientRepository.GetClientsAsync(originTenant, cancellationToken);

        var clientsInTargetRealm = (await clientRepository.GetClientsAsync(request.TargetTenant, cancellationToken)).Data;

        foreach (var originClient in originClients?.Data ?? [])
        {
            var originClientRoles = (await clientRoleRepository.GetRolesForClientAsync(originClient.Id, originTenant, cancellationToken)).Data;

            if (clientsInTargetRealm.Any(c => c.ClientId == originClient.ClientId))
            {
                var targetClient = clientsInTargetRealm.First(c => c.ClientId == originClient.ClientId);
                var targetClientRoles = (await clientRoleRepository.GetRolesForClientAsync(targetClient.Id, request.TargetTenant, cancellationToken)).Data;

                foreach (var originClientRole in originClientRoles ?? [])
                {
                    if (!targetClientRoles.Any(r => r.Name == originClientRole.Name))
                    {
                        await clientRoleRepository.AddClientRoleAsync(targetClient.Id,
                                originClientRole.Name,
                                originClientRole?.Description ?? "",
                                request.TargetTenant,
                                cancellationToken);
                    }
                }
            }
        }

        return Result<bool>.Success(true);
    }
}