using Feijuca.Auth.Application.Commands.Client;
using Feijuca.Auth.Application.Queries.Clients;
using Feijuca.Auth.Application.Requests.Client;
using Feijuca.Auth.Attributes;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Api.Controllers;

[Route("api/v1/clients")]
[ApiController]
[Authorize]
public class ClientsController(ICommandMediator commandMediator, IQueryMediator queryMediator ) : ControllerBase
{
    /// <summary>
    /// Recovers all clients registered in the realm.
    /// </summary>
    /// <returns>
    /// A 200 OK status code along with the list of clients if the operation is successful;
    /// otherwise, a 400 Bad Request status code with an error message, or a 500 Internal Server Error status code if something goes wrong.
    /// </returns>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiReader")]
    public async Task<IActionResult> GetClients(CancellationToken cancellationToken)
    {
        var result = await queryMediator.QueryAsync(new GetAllClientsQuery(), cancellationToken);

        if (result.Any())
        {
            return Ok(result);
        }

        return BadRequest("Error while getting all clients.");
    }

    /// <summary>
    /// Creates a new client in the realm.
    /// </summary>
    /// <returns>
    /// A 200 OK status code along with the list of clients if the operation is successful;
    /// otherwise, a 400 Bad Request status code with an error message, or a 500 Internal Server Error status code if something goes wrong.
    /// </returns>
    /// <param name="addClient">The body related to the client that will be created.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> CreateClient([FromBody] AddClientRequest addClient, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new AddClientCommand(addClient), cancellationToken);

        if (result.IsSuccess)
        {
            return Created();
        }

        return BadRequest("Error while tried created client.");
    }

    /// <summary>
    /// Synchronizes a client in the realm.
    /// </summary>
    /// <returns>
    /// A 200 OK status code along with the list of clients if the operation is successful;
    /// otherwise, a 400 Bad Request status code with an error message, or a 500 Internal Server Error status code if something goes wrong.
    /// </returns>
    /// <param name="targetTenant">The body related to the tenant that will be synchronized.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
    [HttpPost("{targetTenant}/sync")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> SyncClient([FromRoute] string targetTenant, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new SyncClientCommand(targetTenant), cancellationToken);

        if (result.IsSuccess)
        {
            return Created();
        }

        return BadRequest("Error while tried created client.");
    }
}