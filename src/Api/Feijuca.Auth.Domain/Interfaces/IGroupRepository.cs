using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Filters;

namespace Feijuca.Auth.Domain.Interfaces
{
    public interface IGroupRepository : IBaseRepository
    {
        Task<Result<IEnumerable<Group>>> GetAllAsync(CancellationToken cancellationToken);
        Task<Result<IEnumerable<Group>>> GetGroupByNameAsync(string? groupName, string? tenant, CancellationToken cancellationToken);
        Task<Result<string>> CreateAsync(string name, string tenant, Dictionary<string, string[]> attributes, CancellationToken cancellationToken);
        Task<Result> UpdateAsync(Group group, CancellationToken cancellationToken);
        Task<Result> DeleteAsync(string id, CancellationToken cancellationToken);
        Task<Result<IEnumerable<Entities.User>>> GetUsersInGroupAsync(string id, UserFilters userFilters, int totalUsers, CancellationToken cancellationToken);
    }
}
