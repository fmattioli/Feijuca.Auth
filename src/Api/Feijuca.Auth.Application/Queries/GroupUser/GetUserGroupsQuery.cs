using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Models;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.GroupUser;

public record GetUserGroupsQuery(Guid UserId) : IQuery<Result<IEnumerable<GroupResponse>>>;