using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Interfaces;
using LiteBus.Commands.Abstractions;
using Feijuca.Auth.Providers;

namespace Feijuca.Auth.Application.Commands.GroupRoles;

public class AddClientRoleToGroupCommandHandler(IGroupRepository groupRepository, 
    IGroupRolesRepository roleGroupRepository, 
    IClientRoleRepository roleRepository,
    ITenantProvider tenantProvider) 
    : ICommandHandler<AddClientRoleToGroupCommand, Result<bool>>
{
    private readonly IGroupRepository _groupRepository = groupRepository;
    private readonly IGroupRolesRepository _roleGroupRepository = roleGroupRepository;
    private readonly IClientRoleRepository _roleRepository = roleRepository;

    public async Task<Result<bool>> HandleAsync(AddClientRoleToGroupCommand request, CancellationToken cancellationToken = default)
    {
        var groupsResult = await _groupRepository.GetAllAsync(cancellationToken);
        var rolesResult = await _roleRepository.GetRolesForClientAsync(request.AddRoleToGroupRequest.ClientId, tenantProvider.Tenant.Name, cancellationToken);

        if (groupsResult.IsSuccess && rolesResult.IsSuccess)
        {
            var group = groupsResult.Data.FirstOrDefault(x => x.Id == request.GroupId);
            var role = rolesResult.Data.FirstOrDefault(x => x.Id == request.AddRoleToGroupRequest.RoleId);

            if (group != null && role != null)
            {

                var result = await _roleGroupRepository.AddClientRoleToGroupAsync(
                    group.Id,
                    request.AddRoleToGroupRequest.ClientId,
                    role.Id,
                    role.Name,
                    tenantProvider.GetRequestedTenant()!.Name,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return Result<bool>.Success(true);
                }
            }
        }

        return Result<bool>.Failure(GroupRolesErrors.ErrorAddRoleToGroup);
    }
}
