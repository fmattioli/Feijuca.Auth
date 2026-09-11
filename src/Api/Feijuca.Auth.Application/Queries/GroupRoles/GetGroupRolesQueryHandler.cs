using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Application.Responses;
using LiteBus.Queries.Abstractions;
using Feijuca.Auth.Providers;

namespace Feijuca.Auth.Application.Queries.GroupRoles
{
    public class GetGroupRolesQueryHandler(IGroupRolesRepository roleGroupRepository, ITenantProvider tenantProvider) : IQueryHandler<GetGroupRolesQuery, Result<IEnumerable<GroupRolesResponse>>>
    {
        private readonly IGroupRolesRepository _roleGroupRepository = roleGroupRepository;

        public async Task<Result<IEnumerable<GroupRolesResponse>>> HandleAsync(GetGroupRolesQuery request, CancellationToken cancellationToken = default)
        {
            var groupsRolesResult = await _roleGroupRepository.GetGroupRolesAsync(request.GroupId, tenantProvider.Tenant.Name, cancellationToken);

            if (groupsRolesResult.IsSuccess)
            {
                return Result<IEnumerable<GroupRolesResponse>>.Success(groupsRolesResult.Data.ToResponse());
            }

            return Result<IEnumerable<GroupRolesResponse>>.Failure(GroupErrors.GetUsersInGroupsError);
        }
    }
}
