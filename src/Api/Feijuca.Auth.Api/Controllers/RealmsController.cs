using Feijuca.Auth.Application.Commands.Realm;
using Feijuca.Auth.Application.Queries.Realm;
using Feijuca.Auth.Application.Requests.Realm;
using Feijuca.Auth.Attributes;
using Feijuca.Auth.Http.Requests;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Api.Controllers;

[Route("api/v1/realms")]
[ApiController]
[Authorize]
public class RealmsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
{
    /// <summary>
    /// Retrieves all registred realms.
    /// </summary>
    /// <returns>
    /// A 200 Ok status code containing all realms registred in Keycloak.
    /// </returns>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> GetRealms(CancellationToken cancellationToken)
    {
        var result = await queryMediator.QueryAsync(new GetRealmsQuery(), cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// This endpoint has the purpose to replicate clients/client-scopes to another existing realm (tenant).
    /// </summary>
    /// <returns>
    /// A 201 Created status code containing a realm config if the request is successful;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <param name="replicateRealmRequest">Use this parameter to handle what are the things that you should replicate. Inform the clients name, scope names and so on...</param>
    [HttpPost]
    [Route("replicate", Name = nameof(ReplicateRealm))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> ReplicateRealm([FromBody] ReplicateRealmRequest replicateRealmRequest, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new ReplicateRealmCommand(replicateRealmRequest), cancellationToken);

        if (result.IsSuccess)
        {
            return Created($"/replicate", result.Data);
        }

        return BadRequest(result.Error);
    }

    [HttpPost]
    [Route("enable", Name = nameof(EnableRealm))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> EnableRealm([FromBody] EnableRealmRequest enableRealmRequest, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new EnableRealmCommand(enableRealmRequest), cancellationToken);

        if (result.IsSuccess)
        {
            return Created($"/enable", result.Data);
        }

        return BadRequest(result.Error);
    }

    /// <summary>
    /// Create a new realm on Keycloak
    /// </summary>
    /// <returns>
    /// A 201 Created status code meaning that the realm was created with succesfull.;
    /// otherwise, a 400 Bad Request status code with an error message.
    /// </returns>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> that can be used to signal cancellation for the operation.</param>
    /// <param name="realm">The name of the realm.</param>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequiredRole("Feijuca.ApiWriter")]
    public async Task<IActionResult> AddRealm([FromBody] AddRealmRequest realm, CancellationToken cancellationToken)
    {
        var result = await commandMediator.SendAsync(new AddRealmsCommand([realm]), cancellationToken);

        if (result.IsSuccess)
        {
            return Created("/api/v1/realm", result.Data);
        }

        return BadRequest(result.Error);
    }
}
