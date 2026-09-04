using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Realm;

public class UpdateRealmSettingsCommandHandler(ITenantProvider tenantProvider,
    IRealmRepository realmRepository) : ICommandHandler<UpdateRealmSettingsCommand, Result>
{
    public async Task<Result> HandleAsync(UpdateRealmSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var realm = await realmRepository.GetAsync(tenantProvider.Tenant.Name, cancellationToken);

        var newRealm = realm.Data.ToUpdateSettings(command.Request);

        var result = await realmRepository.UpdateRealmAsync(tenantProvider.Tenant.Name, newRealm, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Failure(result.Error);
        }

        return Result.Success();
    }
}