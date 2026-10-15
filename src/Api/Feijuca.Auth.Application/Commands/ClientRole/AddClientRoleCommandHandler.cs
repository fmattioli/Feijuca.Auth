using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using Microsoft.Extensions.Logging;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public class AddClientRoleCommandHandler(IClientRoleRepository clientRolesRepository,
    ITenantProvider tenantProvider,
    ILogger<AddClientRoleCommandHandler> logger) : ICommandHandler<AddClientRoleCommand, Result<bool>>
{
    private readonly IClientRoleRepository _roleRepository = clientRolesRepository;

    public async Task<Result<bool>> HandleAsync(AddClientRoleCommand request, CancellationToken cancellationToken)
    {
        if (!(request.AddClientRolesRequest.ClientRoles?.Any() ?? false))
        {
            logger.LogWarning("No client roles provided in the request.");
            return Result<bool>.Failure(RoleErrors.AddRoleErrors);
        }

        foreach (var clientRole in request.AddClientRolesRequest.ClientRoles)
        {
            var result = await _roleRepository.AddClientRoleAsync(
                clientRole.ClientId,
                clientRole.Name,
                clientRole.Description,
                tenantProvider.GetRequestedTenant()!.Name,
                cancellationToken);

            if (!result.IsSuccess)
            {
                return Result<bool>.Failure(RoleErrors.AddRoleErrors);
            }
        }

        return Result<bool>.Success(true);
    }
}