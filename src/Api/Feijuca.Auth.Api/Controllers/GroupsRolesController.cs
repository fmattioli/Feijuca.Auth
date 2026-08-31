using Feijuca.Auth.Application.Commands.GroupRoles;
using Feijuca.Auth.Application.Queries.GroupRoles;
using Feijuca.Auth.Application.Requests.GroupRoles;
using Feijuca.Auth.Attributes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Feijuca.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feijuca.Auth.Http.Requests;

namespace Feijuca.Auth.Api.Controllers
{
    [Route("api/v1/groups-roles")]
    [ApiController]
    [Authorize]
    public class GroupsRolesController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
    {
        /// <summary>
        /// Adds a role to a specific group in the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 201 Created status code if the role is successfully added to the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="id">The unique identifier of the group to which the role will be added.</param>
        /// <param name="addRoleToGroup">An object of type <see cref="T:Feijuca.Auth.Common.Models.AddClientRoleToGroupRequest"/> containing the details of the role to be added to the group.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpPost("{id:guid}/role", Name = nameof(AddRoleToGroup))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiWriter")]
        public async Task<IActionResult> AddRoleToGroup(
            [FromRoute] string id,
            [FromBody] AddClientRoleToGroupRequest addRoleToGroup,
            CancellationToken cancellationToken)
        {
            var result = await commandMediator.SendAsync(new AddClientRoleToGroupCommand(id, addRoleToGroup), cancellationToken);

            if (result.IsSuccess)
            {
                return CreatedAtAction(nameof(GetGroupRoles), new { id }, null);
            }

            return BadRequest(Result<string>.Failure(result.Error));
        }

        /// <summary>
        /// Removes a role from a specific group in the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 204 No Content status code if the role is successfully removed from the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="groupid">The unique identifier of the group from which the role will be removed.</param>
        /// <param name="deleteRoleToGroup">An object of type <see cref="T:Feijuca.Auth.Common.Models.RemoveClientRoleToGroupRequest"/> containing the details of the role to be deleted to the group.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpDelete("{groupid}/role", Name = nameof(RemoveRoleFromGroup))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiWriter")]
        public async Task<IActionResult> RemoveRoleFromGroup(
            [FromRoute] string groupid,
            [FromBody] RemoveClientRoleToGroupRequest deleteRoleToGroup,
            CancellationToken cancellationToken)
        {
            var result = await commandMediator.SendAsync(new RemoveRoleFromGroupCommand(groupid, deleteRoleToGroup), cancellationToken);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(Result<string>.Failure(result.Error));
        }

        /// <summary>
        /// Retrieves the roles associated with a specific group in the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 200 OK status code with a list of roles associated with the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="id">The unique identifier of the group whose roles are being retrieved.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpGet("{id:guid}/roles", Name = nameof(GetGroupRoles))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiReader")]
        public async Task<IActionResult> GetGroupRoles(
            [FromRoute] string id,
            CancellationToken cancellationToken)
        {
            var result = await queryMediator.QueryAsync(new GetGroupRolesQuery(id), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            return BadRequest(Result<string>.Failure(result.Error));
        }
    }
}
