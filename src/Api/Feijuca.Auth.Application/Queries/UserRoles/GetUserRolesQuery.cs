using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Models;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.UserRoles;

public record GetUserRolesQuery(string UserId) : IQuery<Result<IEnumerable<UserRolesResponse>>>;
