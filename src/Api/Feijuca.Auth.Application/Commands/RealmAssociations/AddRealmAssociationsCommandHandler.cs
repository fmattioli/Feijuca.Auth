using Feijuca.Auth.Common;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.RealmAssociations;

public class AddRealmAssociationsCommandHandler(IRealmRepository realmRepository, IClientScopesRepository clientScopesRepository) : ICommandHandler<AddRealmAssociationsCommand, Result>
{
    public async Task<Result> HandleAsync(AddRealmAssociationsCommand command, CancellationToken cancellationToken = default)
    {
        var realmResult = await realmRepository.GetAsync(command.AddRealmAssociationsRequest.TargetRealm, cancellationToken);

        if (realmResult.IsFailure)
        {
            return Result.Failure(realmResult.Error);
        }

        var targetClientScopes = await clientScopesRepository.GetClientScopesAsync(command.AddRealmAssociationsRequest.TargetRealm, cancellationToken);
        var targetFeijucaClientScope = targetClientScopes.FirstOrDefault(x => x.Name == Constants.FeijucaApiClientName)!;

        var allowedTenantsProtocolMapper = targetFeijucaClientScope.ProtocolMappers?
            .FirstOrDefault(x => x.Name == Constants.AllowedTenantsProtocolMapperName);

        if (allowedTenantsProtocolMapper == null)
        {
            return Result.Failure(RealmErrors.AllowedTenantsMapperNotFoundError);
        }

        var allowedTenantsValue = allowedTenantsProtocolMapper.Config.GetValueOrDefault(Constants.AllowedTenantsConfigKey);

        foreach (var realmToAssociate in command.AddRealmAssociationsRequest.RealmsToAssociate)
        {
            if (!allowedTenantsValue?.Contains(realmToAssociate) ?? false)
            {
                allowedTenantsValue += $",{realmToAssociate}";
            }
        }

        allowedTenantsProtocolMapper!.Config[Constants.AllowedTenantsConfigKey] = allowedTenantsValue!;

        return await clientScopesRepository.UpdateAllowedTenantsMapperAsync(
            targetFeijucaClientScope.Id!,
            command.AddRealmAssociationsRequest.TargetRealm,
            allowedTenantsProtocolMapper!,
            cancellationToken);
    }
}
