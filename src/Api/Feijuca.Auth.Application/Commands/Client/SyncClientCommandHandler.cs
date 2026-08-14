using Feijuca.Auth.Common;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Client;

public class SyncClientCommandHandler(ITenantProvider tenantProvider,
    IClientRepository clientRepository,
    IClientRoleRepository clientRoleRepository,
    IGroupRolesRepository groupRolesRepository,
    IGroupRepository groupRepository) : ICommandHandler<SyncClientCommand, Result>
{
    public async Task<Result> HandleAsync(SyncClientCommand request, CancellationToken cancellationToken = default)
    {
        var originTenant = tenantProvider.Tenant.Name;
        var originClients = await clientRepository.GetClientsAsync(originTenant, cancellationToken);

        var adminGroupResult = await groupRepository.GetGroupByNameAsync(Constants.AdminGroupName, request.TargetTenant, cancellationToken);
        var adminGroupId = adminGroupResult.Data!.FirstOrDefault()!.Id;

        var clientsInTargetRealm = (await clientRepository.GetClientsAsync(request.TargetTenant, cancellationToken)).Data;

        foreach (var client in originClients?.Data ?? [])
        {
            if (!clientsInTargetRealm.Any(c => c.ClientId == client.ClientId))
            {
                var clientId = (await clientRepository.CreateClientAsync(client, request.TargetTenant, cancellationToken)).Data;

                await AssociatedRulesToTheClientAsync(request.TargetTenant, originTenant, client, clientId, cancellationToken);
                await AssociateClientRulesToTheGroupAsync(request.TargetTenant, adminGroupId!, clientId, cancellationToken);
            }
        }

        return Result.Success();
    }

    private async Task AssociatedRulesToTheClientAsync(string targetTenant, string originTenant, ClientEntity client, string clientId, CancellationToken cancellationToken)
    {
        var originClientRoles = await clientRoleRepository.GetRolesForClientAsync(client.Id, originTenant, cancellationToken);

        foreach (var clientRole in originClientRoles.Data)
        {
            await clientRoleRepository.AddClientRoleAsync(clientId,
                clientRole.Name,
                clientRole?.Description ?? "",
                targetTenant,
                cancellationToken);
        }
    }

    private async Task AssociateClientRulesToTheGroupAsync(string targetTenant, string adminGroupId, string clientId, CancellationToken cancellationToken)
    {
        var targetClientRulesAdded = await clientRoleRepository.GetRolesForClientAsync(clientId, targetTenant, cancellationToken);

        foreach (var targetClientRuleAdded in targetClientRulesAdded.Data)
        {
            await groupRolesRepository.AddClientRoleToGroupAsync(adminGroupId,
                clientId,
                targetClientRuleAdded.Id,
                targetClientRuleAdded.Name,
                targetTenant,
                cancellationToken);
        }
    }

}