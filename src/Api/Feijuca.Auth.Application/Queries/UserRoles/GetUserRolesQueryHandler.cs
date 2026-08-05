using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.UserRoles;

public class GetUserRolesQueryHandler(IUserRolesRepository userRolesRepository) : IQueryHandler<GetUserRolesQuery, Result<IEnumerable<UserRolesResponse>>>
{
    public async Task<Result<IEnumerable<UserRolesResponse>>> HandleAsync(GetUserRolesQuery query, CancellationToken cancellationToken = default)
    {
        var userRolesResult = await userRolesRepository.GetUserRolesAsync(query.UserId, cancellationToken);

        if (userRolesResult.IsSuccess)
        {
            return Result<IEnumerable<UserRolesResponse>>.Success(userRolesResult.Data.ToUserRolesResponse());
        }

        return Result<IEnumerable<UserRolesResponse>>.Failure(UserRolesErrors.ErrorGetUserRoles);
    }
}
