using Feijuca.Auth.Application.Commands.GroupUser;
using Feijuca.Auth.Application.Queries.GroupUser;
using Feijuca.Auth.Application.Requests.GroupUsers;
using Feijuca.Auth.Attributes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Feijuca.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Feijuca.Auth.Http.Requests;

namespace Feijuca.Auth.Api.Controllers
{
    [Route("api/v1")]
    [ApiController]
    [Authorize]
    public class GroupsUsersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
    {
        /// <summary>
        /// Adds a user to a specific group in the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 204 No Content status code if the user is successfully added to the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="addUserToGroupRequest">An object of type <see cref="T:Feijuca.Auth.Common.Models.AddUserToGroupRequest"/> containing the user and group IDs for the operation.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpPost("groups/users", Name = nameof(AddUserToGroup))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiWriter")]
        public async Task<IActionResult> AddUserToGroup([FromBody] AddUserToGroupRequest addUserToGroupRequest,
            CancellationToken cancellationToken)
        {
            var result = await commandMediator.SendAsync(new AddUserToGroupCommand(addUserToGroupRequest.UserId, addUserToGroupRequest.GroupId), cancellationToken);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(result.Error);
        }

        /// <summary>
        /// Retrieves all users present in a specific group within the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 200 OK status code with a list of users in the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="usersGroup">An object of type <see cref="T:Feijuca.Auth.Common.Models.GetUsersGroupRequest"/> containing the necessary parameters to filter users in the group.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpGet("groups/users", Name = nameof(GetUsersInGroup))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiReader")]
        public async Task<IActionResult> GetUsersInGroup([FromQuery] GetUsersGroupRequest usersGroup, CancellationToken cancellationToken)
        {
            var result = await queryMediator.QueryAsync(new GetUsersGroupQuery(usersGroup), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(result.Data);
            }

            var responseError = Result<string>.Failure(result.Error);
            return BadRequest(responseError);
        }

        /// <summary>
        /// Removes a user from a specific group within the specified Keycloak realm.
        /// </summary>
        /// <returns>
        /// A 204 No Content status code if the user is successfully removed from the group;
        /// otherwise, a 400 Bad Request status code with an error message.
        /// </returns>
        /// <param name="removeUserFromGroup">An object of type <see cref="T:Feijuca.Auth.Common.Models.RemoveUserFromGroupRequest"/> containing the user ID and group ID for the operation.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpDelete("groups/users", Name = nameof(RemoveUserFromGroup))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [RequiredRole("Feijuca.ApiWriter")]
        public async Task<IActionResult> RemoveUserFromGroup([FromBody] RemoveUserFromGroupRequest removeUserFromGroup, CancellationToken cancellationToken)
        {
            var result = await commandMediator.SendAsync(new RemoveUserFromGroupCommand(removeUserFromGroup.UserId, removeUserFromGroup.GroupId), cancellationToken);

            if (result.IsSuccess)
            {
                return NoContent();
            }

            var responseError = Result<string>.Failure(result.Error);
            return BadRequest(responseError);
        }
    }
}
