using Feijuca.Auth.Common;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;
using MongoDB.Driver;

namespace Feijuca.Auth.Application.Commands.RealmAssociations;

internal class RemoveRealmAssociationsCommandHandler(IRealmRepository realmRepository, IClientScopesRepository clientScopesRepository) : ICommandHandler<RemoveRealmAssociationsCommand, Result>
{
    public async Task<Result> HandleAsync(RemoveRealmAssociationsCommand command, CancellationToken cancellationToken = default)
    {
        var realmResult = await realmRepository.GetAsync(command.RemoveRealmAssociationsRequest.TargetRealm, cancellationToken);

        if (realmResult.IsFailure)
        {
            return Result.Failure(realmResult.Error);
        }

        var targetClientScopes = await clientScopesRepository.GetClientScopesAsync(command.RemoveRealmAssociationsRequest.TargetRealm, cancellationToken);
        var targetFeijucaClientScope = targetClientScopes.FirstOrDefault(x => x.Name == Constants.FeijucaApiClientName)!;

        var allowedTenantsProtocolMapper = targetFeijucaClientScope.ProtocolMappers?
            .FirstOrDefault(x => x.Name == Constants.AllowedTenantsProtocolMapperName);

        if (allowedTenantsProtocolMapper == null)
        {
            return Result.Failure(RealmErrors.AllowedTenantsMapperNotFoundError);
        }

        var allowedTenantsValue = allowedTenantsProtocolMapper.Config.GetValueOrDefault(Constants.AllowedTenantsConfigKey);

        var allowedTenantsWithoutDisassociations = allowedTenantsValue?
            .Split(',')
            .Where(x => 
                !command.RemoveRealmAssociationsRequest.RealmsToDisassociate.Contains(x) ||
                x == command.RemoveRealmAssociationsRequest.TargetRealm);

        allowedTenantsProtocolMapper!.Config[Constants.AllowedTenantsConfigKey] = string.Join(',', allowedTenantsWithoutDisassociations!);

        return await clientScopesRepository.UpdateAllowedTenantsMapperAsync(
            targetFeijucaClientScope.Id!,
            command.RemoveRealmAssociationsRequest.TargetRealm,
            allowedTenantsProtocolMapper!,
            cancellationToken); ;
    }
}
