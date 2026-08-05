using Feijuca.Auth.Application.Queries.UserRoles;
using Feijuca.Auth.Attributes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Api.Controllers;

[Route("api/v1/users-roles")]
[ApiController]
[Authorize]
public class UsersRolesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the roles associated with a specific user in the specified Keycloak realm.
    /// </summary>
    /// <returns>
    /// A 200 OK status code with a list of roles associated with the user;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <param name="id">The unique identifier of the user whose roles are being retrieved.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
    [HttpGet("{id:guid}/roles", Name = nameof(GetUserRoles))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiReader")]
    public async Task<IActionResult> GetUserRoles(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var result = await queryMediator.QueryAsync(new GetUserRolesQuery(id), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Error);
    }
}
