using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.UserRoles;

public class AddClientRoleToUserCommandHandler(IUserRepository userRepository,
    IUserRolesRepository userRoleRepository,
    IClientRoleRepository roleRepository,
    ITenantProvider tenantProvider)
    : ICommandHandler<AddClientRoleToUserCommand, Result>
{
    public async Task<Result> HandleAsync(AddClientRoleToUserCommand command, CancellationToken cancellationToken = default)
    {
        var userResult = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        var rolesResult = await roleRepository.GetRolesForClientAsync(command.AddRoleToUserRequest.ClientId, tenantProvider.Tenant.Name, cancellationToken);

        if (userResult.IsSuccess && rolesResult.IsSuccess)
        {
            var role = rolesResult.Data.FirstOrDefault(x => x.Id == command.AddRoleToUserRequest.RoleId);

            if (role != null)
            {

                var result = await userRoleRepository.AddClientRoleToUserAsync(
                    userResult.Data.Id.ToString(),
                    command.AddRoleToUserRequest.ClientId,
                    role.Id,
                    role.Name,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return Result.Success();
                }
            }
        }

        return Result.Failure(UserRolesErrors.ErrorAddRoleToUser);
    }
}
