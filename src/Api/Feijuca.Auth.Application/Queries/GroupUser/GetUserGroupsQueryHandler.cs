using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.GroupUser;

public class GetUserGroupsQueryHandler(IGroupUsersRepository usersRepository) : IQueryHandler<GetUserGroupsQuery, Result<IEnumerable<GroupResponse>>>
{
    public async Task<Result<IEnumerable<GroupResponse>>> HandleAsync(GetUserGroupsQuery request, CancellationToken cancellationToken = default)
    {
        var result = await usersRepository.GetUserGroupsAsync(request.UserId, cancellationToken);

        var response = result.Data.ToResponse();

        if (result.IsFailure)
        {
            return Result<IEnumerable<GroupResponse>>.Failure(result.Error);
        }

        return Result<IEnumerable<GroupResponse>>.Success(response);
    }
}