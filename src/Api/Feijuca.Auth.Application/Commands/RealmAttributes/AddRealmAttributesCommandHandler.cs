using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.RealmAttributes;

public class AddRealmAttributesCommandHandler(IRealmRepository realmRepository, ITenantProvider tenantProvider) : ICommandHandler<AddRealmAttributesCommand, Result>
{
    public async Task<Result> HandleAsync(AddRealmAttributesCommand command, CancellationToken cancellationToken = default)
    {
        var realm = await realmRepository.GetAsync(tenantProvider.Tenant.Name, cancellationToken);
        if (realm.IsSuccess)
        {
            var realmNewAttributes = realm.Data.Attributes;
            foreach (var item in command.AddRealmAttributesRequest.Attributes)
            {
                realmNewAttributes.TryAdd(item.Key, item.Value);
            }

            realm.Data.Attributes = realmNewAttributes;

            var result = await realmRepository.UpdateRealmAsync(tenantProvider.Tenant.Name, realm.Data, cancellationToken);

            if (result.IsSuccess)
            {
                return Result.Success();
            }

            return Result.Failure(result.Error);
        }

        return Result.Failure(realm.Error);
    }
}