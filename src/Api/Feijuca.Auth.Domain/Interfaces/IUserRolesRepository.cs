using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Domain.Interfaces;

public interface IUserRolesRepository : IBaseRepository
{
    Task<Result<IEnumerable<ClientMapping>>> GetUserRolesAsync(string userId, CancellationToken cancellationToken);
}
