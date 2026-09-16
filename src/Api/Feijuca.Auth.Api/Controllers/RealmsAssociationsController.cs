using Feijuca.Auth.Application.Commands.RealmAssociations;
using Feijuca.Auth.Application.Commands.RealmAttributes;
using Feijuca.Auth.Application.Requests.RealmAssociations;
using Feijuca.Auth.Application.Requests.RealmAttributes;
using Feijuca.Auth.Attributes;
using LiteBus.Commands.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Api.Controllers;

[Route("api/v1/realms-associations")]
[ApiController]
[Authorize]
public class RealmsAssociationsController(ICommandMediator commandMediator) : ControllerBase
{
    /// <summary>
    /// Adds a association between realms in Keycloak.
    /// </summary>
    /// <param name="addRealmAssociationsRequest">The request object containing the necessary details to add association to the realm.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <returns>
    /// A 201 Created status code if the associations are successfully created;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    [HttpPost]
    [EndpointDescription("This endpoint add new realms associations related to the an existing realm.")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> AddAssociations([FromBody] AddRealmAssociationsRequest addRealmAssociationsRequest,
        CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new AddRealmAssociationsCommand(addRealmAssociationsRequest), cancellationToken);

        if (result.IsSuccess)
        {
            return Created();
        }

        return BadRequest(result.Error);
    }
}
