using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Providers;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.Clients
{
    public class GetAllClientsQueryHandler(IClientRepository clientRepository, ITenantProvider tenantProvider) : IQueryHandler<GetAllClientsQuery, IEnumerable<ClientResponse>>
    {
        private readonly IClientRepository _clientRepository = clientRepository;

        public async Task<IEnumerable<ClientResponse>> HandleAsync(GetAllClientsQuery request, CancellationToken cancellationToken = default)
        {
            var clientsResult = await _clientRepository.GetClientsAsync(tenantProvider.GetRequestedTenant()!.Name, cancellationToken);

            if (clientsResult.IsSuccess)
            {
                var clients = clientsResult.Data.Select(x => new ClientResponse(x.Enabled, x.Id, x.ClientId));
                return clients;
            }

            return [];
        }
    }
}
