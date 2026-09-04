using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Entities;

namespace Feijuca.Auth.Domain.Interfaces
{
    public interface IRealmService
    {
        Task<Result<bool>> AddNewRealmAsync(RealmEntity realm, CancellationToken cancellationToken);
    }
}