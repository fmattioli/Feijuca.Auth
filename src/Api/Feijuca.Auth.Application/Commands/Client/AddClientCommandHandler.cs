using DnsClient.Internal;
using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace Feijuca.Auth.Application.Commands.Client
{
    public class AddClientCommandHandler(IRealmRepository realmRepository,
        IClientRepository clientRepository,
        ITenantProvider tenantService,
        ILogger<AddClientCommandHandler> logger) : ICommandHandler<AddClientCommand, Result>
    {
        public async Task<Result> HandleAsync(AddClientCommand request, CancellationToken cancellationToken)
        {
            var client = request.AddClientRequest.ToClientEntity();

            var result = await clientRepository.CreateClientAsync(client, tenantService.Tenant.Name, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to create client in tenant: {Tenant}. Error: {Error}", tenantService.Tenant.Name, result.Error);
                return Result.Failure(result.Error);
            }

            if (request.AddClientRequest.AllTenants)
            {
                var realms = await realmRepository.GetAllAsync(cancellationToken);

                var tenants = realms.Where(r => r.Realm != tenantService.Tenant.Name).Select(r => r.Realm);

                foreach (var tenant in tenants)
                {
                    var clients = await clientRepository.GetClientsAsync(tenant, cancellationToken);

                    if (clients.Data.Any(x => x.ClientId == request.AddClientRequest.ClientId))
                    {
                        logger.LogWarning("Client already exists in tenant: {Tenant}", tenant);
                        continue;
                    }

                    var response = await clientRepository.CreateClientAsync(client, tenant, cancellationToken);

                    if (!response.IsSuccess)
                    {
                        logger.LogError("Failed to create client in tenant: {Tenant}. Error: {Error}", tenant, response.Error);
                        return Result.Failure(response.Error);
                    }

                    logger.LogInformation("Creating client in tenant: {Tenant}", tenant);
                }

                return Result.Success();
            }

            logger.LogInformation("Creating client in tenant: {Tenant}", tenantService.Tenant.Name);
            return Result.Success();
        }
    }
}