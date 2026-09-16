using Feijuca.Auth.Domain.Entities;

namespace Feijuca.Auth.Domain.Interfaces
{
    public interface IClientScopesRepository : IBaseRepository
    {
        Task<string?> AddClientScopesAsync(ClientScopeEntity clientScopesEntity, string tenant, CancellationToken cancellationToken);

        Task<bool> AddUserPropertyMapperAsync(string clientScopeId,
            string userPropertyName,
            string claimName,
            string tenant,
            CancellationToken cancellationToken);

        Task<bool> AddClientScopeToClientAsync(string clientId,
            string tenant,
            string clientScopeId,
            bool isOptional,
            CancellationToken cancellationToken);

        Task<IEnumerable<ClientScopeEntity>> GetClientScopesAsync(string tenant, CancellationToken cancellationToken);
        Task<bool> AddAudienceMapperAsync(string clientScopeId, string tenant, CancellationToken cancellationToken);
        Task<bool> AddGroupMembershipMapperAsync(string clientScopeId, string tenant, CancellationToken cancellationToken);
        Task<bool> AddAllowedTenantsMapperAsync(string clientScopeId, string tenant, IEnumerable<string> allowedTenants, CancellationToken cancellationToken);
        Task<ClientScopeEntity> GetClientScopeProfileAsync(string tenant, CancellationToken cancellationToken);
        Task<IEnumerable<ClientScopeEntity>> GetClientScopesAssociatedToTheClientAsync(string tenant, string clientId, CancellationToken cancellationToken);
    }
}
