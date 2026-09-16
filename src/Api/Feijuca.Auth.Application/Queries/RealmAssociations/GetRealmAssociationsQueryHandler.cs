using Feijuca.Auth.Common;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Queries.Abstractions;
using MongoDB.Driver;

namespace Feijuca.Auth.Application.Queries.RealmAssociations;

public class GetRealmAssociationsQueryHandler(IClientScopesRepository clientScopesRepository, ITenantProvider tenantProvider) : IQueryHandler<GetRealmAssociationsQuery, Result<IEnumerable<string>>>
{
    public async Task<Result<IEnumerable<string>>> HandleAsync(GetRealmAssociationsQuery query, CancellationToken cancellationToken = default)
    {
        var clientScopes = await clientScopesRepository.GetClientScopesAsync(tenantProvider.GetTenantContext().Name, cancellationToken);
        var feijucaClientScope = clientScopes.FirstOrDefault(x => x.Name == Constants.FeijucaApiClientName)!;

        var allowedTenantsProtocolMapper = feijucaClientScope.ProtocolMappers?.FirstOrDefault(x => x.Name == Constants.AllowedTenantsProtocolMapperName);

        if (allowedTenantsProtocolMapper == null)
        {
            return Result<IEnumerable<string>>.Failure(RealmErrors.AllowedTenantsMapperNotFoundError);
        }

        var allowedTenantsValue = allowedTenantsProtocolMapper.Config.GetValueOrDefault(Constants.AllowedTenantsConfigKey);

        return Result<IEnumerable<string>>.Success(allowedTenantsValue!.Split(','));
    }
}
