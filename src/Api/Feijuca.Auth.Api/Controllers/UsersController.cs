using Feijuca.Auth.Application.Commands.User;
using Feijuca.Auth.Application.Queries.Users;
using Feijuca.Auth.Application.Requests.Auth;
using Feijuca.Auth.Application.Requests.User;
using Feijuca.Auth.Attributes;
using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Feijuca.Auth.Api.Controllers;

[Route("api/v1/users")]
[ApiController]
public class UsersController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    /// <summary>
    /// Retrieves all users existing in the specified Keycloak realm.
    /// </summary>
    /// <param name="getUsersRequest">The request object containing filtering and pagination parameters for user retrieval.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 200 OK status code with the list of users if the operation is successful;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <response code="200">The list of users was retrieved successfully.</response>
    /// <response code="400">The request was invalid or could not be processed.</response>
    /// <response code="401">The request lacks valid authentication credentials.</response>
    /// <response code="403">The request was understood, but the server is refusing to fulfill it due to insufficient permissions.</response>
    /// <response code="500">An internal server error occurred while processing the request.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    [RequiredRole("Feijuca.ApiReader")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest getUsersRequest, CancellationToken cancellationToken)
    {
        var result = await queryMediator.QueryAsync(new GetUsersQuery(getUsersRequest), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Adds a new user to the specified Keycloak realm.
    /// </summary>
    /// <param name="addUserRequest">The request object containing the necessary details to create the user.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <param name="tenant">The Tenant that the user belongs to.</param>
    /// <returns>
    /// A 201 Created status code if the user is successfully created;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <response code="201">The user was created successfully.</response>
    /// <response code="400">The request was invalid or could not be processed.</response>
    /// <response code="401">The request lacks valid authentication credentials.</response>
    /// <response code="403">The request was understood, but the server is refusing to fulfill it due to insufficient permissions.</response>
    /// <response code="500">An internal server error occurred while processing the request.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromHeader] string tenant, [FromBody] AddUserRequest addUserRequest,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new AddUserCommand(tenant, addUserRequest), cancellationToken);

        if (result.IsSuccess)
        {
            return Created($"/api/v1/users", result.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Update an existing user in a Keycloak realm.
    /// </summary>
    /// <param name="id">The user id that should have the attributes updated.</param>
    /// <param name="addUserRequest">The user object field that will be updated.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 202Accepted status code if the authentication is successful, along with the JWT token and user attributes was updated;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> UpdateUser([FromQuery] Guid id, [FromBody] AddUserRequest addUserRequest,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new UpdateUserCommand(id, addUserRequest), cancellationToken);

        if (result.IsSuccess)
        {
            var response = Result<bool>.Success(true);
            return Accepted(response.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Deletes an existing user from the specified Keycloak realm.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be deleted.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 204 No Content status code if the user is successfully deleted;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpDelete]
    [Route("{id}", Name = nameof(DeleteUser))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    [Authorize]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new DeleteUserCommand(id), cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Revokes all active sessions for a specified user in Keycloak.
    /// </summary>
    /// <param name="id">The unique identifier of the user whose sessions are to be revoked.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 202 Accepted status code if the sessions are successfully revoked;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpPost]
    [Route("revoke-session", Name = nameof(RevokeUserSessions))]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Authorize]
    public async Task<IActionResult> RevokeUserSessions([FromQuery] Guid id, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new RevokeUserSessionsCommand(id), cancellationToken);

        if (result.IsSuccess)
        {
            return Accepted();
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Logs out a user and invalidates their session.
    /// </summary>
    /// <param name="logoutUserRequest">The request containing the refresh token for the user session to be invalidated.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 200 OK status code if the logout is successful;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpPost]
    [Route("logout", Name = nameof(Logout))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest logoutUserRequest,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new SignoutCommand(logoutUserRequest.RefreshToken), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new { Message = "Logout successful" });
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Authenticates a user and returns a valid JWT token along with user details.
    /// </summary>
    /// <param name="tenant">The Tenant name related to the realm.</param>
    /// <param name="loginUserRequest">The request containing the user's login credentials.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 200 OK status code if the authentication is successful, along with the JWT token and user details;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpPost]
    [Route("login", Name = nameof(Login))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromHeader] string tenant, [FromBody] LoginUserRequest loginUserRequest,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new LoginCommand(tenant, loginUserRequest), cancellationToken);

        if (result.IsSuccess)
        {
            var response = Result<TokenDetailsResponse>.Success(result.Data);
            return Ok(response.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Decodes the JWT token and returns the details related to the user logged.
    /// </summary>
    /// <returns>
    /// A 200 OK status code with the user's details if the token is successfully decoded;
    /// otherwise, a 400 Bad Request status code if the token is invalid or if the user is not authenticated.
    /// </returns>
    [HttpGet]
    [Route("decode-token", Name = nameof(DecodeToken))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public IActionResult DecodeToken([FromHeader(Name = "Tenant")] string tenant = "1")
    {
        if (HttpContext.User.Identity is not ClaimsIdentity identity)
        {
            return BadRequest();
        }

        var userId = identity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var username = identity.FindFirst("preferred_username")!.Value;
        var fullName = identity.FindFirst("name")!.Value;

        var nameParts = fullName.Split(' ');
        var firstName = nameParts.FirstOrDefault();
        var lastName = nameParts.Length > 1 ? nameParts[^1] : "";

        var userResponse = new UserResponse(Guid.Parse(userId), true, true, username, username, firstName!, lastName!, tenant, false, null!, null!, 0, 0, null, null);
        return Ok(userResponse);
    }

    /// <summary>
    /// Refreshes a valid JWT token and returns the new token along with user details.
    /// </summary>
    /// <param name="request">The request containing the refresh token.</param>
    /// <param name="tenant">The request containing the refresh token.</param>
    /// <param name="cancellationToken">A token that can be used to signal cancellation of the operation.</param>
    /// <returns>
    /// A 200 OK status code with the new token and user details if the refresh operation is successful;
    /// otherwise, a 400 Bad Request status code if the refresh token is invalid or expired.
    /// </returns>
    [HttpPost]
    [Route("refresh-token", Name = nameof(RefreshToken))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromHeader] string tenant, [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new RefreshTokenCommand(tenant, request.RefreshToken), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Updates the password for a specific user within a tenant context.
    /// </summary>
    /// <param name="request">The request body containing the user ID, old password, and new password.</param>
    /// <param name="id">The unique identifier of the user whose password is being updated.</param>
    /// <param name="cancellationToken">A token that can be used to signal cancellation of the operation.</param>
    /// <returns>
    /// A 200 OK status code if the password was successfully updated;
    /// otherwise, a 400 Bad Request status code if the current password is incorrect or validation fails.
    /// </returns>
    [HttpPut]
    [Route("{id}/reset-password", Name = nameof(ResetPassword))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize]
    public async Task<IActionResult> ResetPassword([FromRoute] Guid id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new ResetPasswordCommand(id,request), cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }

        return BadRequest(result.Error);
    }
}