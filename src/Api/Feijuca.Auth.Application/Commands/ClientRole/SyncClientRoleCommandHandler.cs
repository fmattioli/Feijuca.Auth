using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using MongoDB.Driver;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public class SyncClientRoleCommandHandler(ITenantProvider tenantProvider,
    IClientRepository clientRepository,
    IClientRoleRepository clientRoleRepository,
    IRealmRepository realmRepository) : ICommandHandler<SyncClientRoleCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(SyncClientRoleCommand request, CancellationToken cancellationToken = default)
    {
        var originTenant = tenantProvider.Tenant.Name;
        var originClients = await clientRepository.GetClientsAsync(originTenant, cancellationToken);
        IEnumerable<string> targetTenants = request.SyncRoleRequest.TargetTenant != null ? [request.SyncRoleRequest.TargetTenant] : [];

        if (request.SyncRoleRequest.AllTenants)
        {
            var realms = await realmRepository.GetAllAsync(cancellationToken);

            targetTenants = realms.Select(r => r.Realm).Where(r => r != originTenant);
        }

        foreach (var targetTenant in targetTenants)
        {
            var clientsInTargetRealm = (await clientRepository.GetClientsAsync(targetTenant, cancellationToken)).Data;

            foreach (var originClient in originClients?.Data ?? [])
            {
                var originClientRoles = (await clientRoleRepository.GetRolesForClientAsync(originClient.Id, originTenant, cancellationToken)).Data;

                if (clientsInTargetRealm.Any(c => c.ClientId == originClient.ClientId))
                {
                    var targetClient = clientsInTargetRealm.First(c => c.ClientId == originClient.ClientId);
                    var targetClientRoles = (await clientRoleRepository.GetRolesForClientAsync(targetClient.Id, targetTenant, cancellationToken)).Data;

                    foreach (var originClientRole in originClientRoles ?? [])
                    {
                        if (!targetClientRoles.Any(r => r.Name == originClientRole.Name))
                        {
                            await clientRoleRepository.AddClientRoleAsync(targetClient.Id,
                                    originClientRole.Name,
                                    originClientRole?.Description ?? "",
                                    targetTenant,
                                    cancellationToken);
                        }
                    }
                }
            }
        }

        return Result<bool>.Success(true);
    }
}