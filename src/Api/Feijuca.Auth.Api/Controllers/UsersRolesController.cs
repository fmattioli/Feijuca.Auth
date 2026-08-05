using Feijuca.Auth.Application.Commands.UserRoles;
using Feijuca.Auth.Application.Queries.UserRoles;
using Feijuca.Auth.Application.Requests.UserRoles;
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

    /// <summary>
    /// Adds a role to a specific user in the specified Keycloak realm.
    /// </summary>
    /// <returns>
    /// A 201 Created status code if the role is successfully added to the user;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <param name="id">The unique identifier of the user to which the role will be added.</param>
    /// <param name="addRoleToUser">An object of type <see cref="T:Feijuca.Auth.Common.Models.AddClientRoleToUserRequest"/> containing the details of the role to be added to the user.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
    [HttpPost("{id:guid}/role", Name = nameof(AddRoleToUser))]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> AddRoleToUser(
        [FromRoute] string id,
        [FromBody] AddClientRoleToUserRequest addRoleToUser,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new AddClientRoleToUserCommand(id, addRoleToUser), cancellationToken);

        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetUserRoles), new { id }, null);
        }

        return BadRequest(result.Error);
    }
}
