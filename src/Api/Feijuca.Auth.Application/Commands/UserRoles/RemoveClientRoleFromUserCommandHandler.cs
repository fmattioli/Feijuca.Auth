using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using MongoDB.Driver;

namespace Feijuca.Auth.Application.Commands.UserRoles;

public class RemoveClientRoleFromUserCommandHandler(IUserRepository userRepository,
    IUserRolesRepository userRolesRepository,
    IClientRoleRepository roleRepository,
    ITenantProvider tenantProvider) : ICommandHandler<RemoveClientRoleFromUserCommand, Result>
{
    public async Task<Result> HandleAsync(RemoveClientRoleFromUserCommand command, CancellationToken cancellationToken = default)
    {
        var userResult = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        var rolesResult = await roleRepository.GetRolesForClientAsync(command.RemoveRoleFromUserRequest.ClientId, tenantProvider.Tenant.Name, cancellationToken);

        if (userResult.IsSuccess && rolesResult.IsSuccess)
        {
            var role = rolesResult.Data.FirstOrDefault(x => x.Id == command.RemoveRoleFromUserRequest.RoleId);

            if (role != null)
            {
                var result = await userRolesRepository.RemoveRoleFromGroupAsync(
                    userResult.Data.Id.ToString(),
                    command.RemoveRoleFromUserRequest.ClientId,
                    role.Id,
                    role.Name,
                    cancellationToken);

                if (result.IsSuccess)
                {
                    return Result.Success();
                }
            }
        }

        return Result.Failure(UserRolesErrors.RemovingRoleFromUserError);
    }
}
