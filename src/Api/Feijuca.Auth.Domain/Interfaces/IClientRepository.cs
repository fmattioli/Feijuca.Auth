using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Domain.Interfaces
{
    public interface IClientRepository : IBaseRepository
    {
        Task<Result<IEnumerable<ClientEntity>>> GetClientsAsync(string tenant, CancellationToken cancellationToken);

        Task<Result<ClientEntity>> GetClientAsync(string clientId, string tenant, CancellationToken cancellationToken);

        Task<Result<string>> CreateClientAsync(
            ClientEntity client,
            string tenantName,
            CancellationToken cancellationToken);
    }
}
