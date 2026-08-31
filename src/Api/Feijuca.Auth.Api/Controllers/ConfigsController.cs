using Feijuca.Auth.Application.Commands.Client;
using Feijuca.Auth.Application.Commands.ClientRole;
using Feijuca.Auth.Application.Commands.ClientScopeMapper;
using Feijuca.Auth.Application.Commands.ClientScopeProtocol;
using Feijuca.Auth.Application.Commands.ClientScopes;
using Feijuca.Auth.Application.Commands.Config;
using Feijuca.Auth.Application.Commands.Group;
using Feijuca.Auth.Application.Commands.GroupRoles;
using Feijuca.Auth.Application.Commands.GroupUser;
using Feijuca.Auth.Application.Commands.Realm;
using Feijuca.Auth.Application.Commands.User;
using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Application.Queries.Clients;
using Feijuca.Auth.Application.Queries.ClientScopes;
using Feijuca.Auth.Application.Queries.Groups;
using Feijuca.Auth.Application.Queries.Permissions;
using Feijuca.Auth.Application.Requests.Client;
using Feijuca.Auth.Application.Requests.ClientScopes;
using Feijuca.Auth.Application.Requests.Config;
using Feijuca.Auth.Application.Requests.GroupRoles;
using Feijuca.Auth.Application.Requests.Realm;
using Feijuca.Auth.Application.Requests.Role;
using Feijuca.Auth.Application.Requests.User;
using Feijuca.Auth.Common;
using LiteBus.Commands.Abstractions;
using LiteBus.Queries.Abstractions;
using Feijuca.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Feijuca.Auth.Http.Requests;

namespace Feijuca.Auth.Api.Controllers
{
    [Route("api/v1/configs")]
    [ApiController]
    public class ConfigsController(ICommandMediator commandMediator, IQueryMediator queryMediator) : ControllerBase
    {
        /// <summary>
        /// Use this endpoint when you have a realm and wish configure keycloak feijuca inside the realm.
        /// </summary>
        /// <returns>
        /// A 200 OK status code along with the list of groups if the operation is successful;
        /// otherwise, a 400 Bad Request status code with an error message, or a 500 Internal Server Error status code if something goes wrong.
        /// </returns>
        /// <param name="addKeycloakSettings">The body of the configuration.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpPost("existing-realm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureExistingRealm([FromBody] AddKeycloakSettingsRequest addKeycloakSettings, CancellationToken cancellationToken)
        {
            return await AddOrUpdateClientConfigs(addKeycloakSettings, false, cancellationToken);
        }

        /// <summary>
        /// Use this endpoint when you do not have a realm and wish configure keycloak feijuca creating a new realm.
        /// </summary>
        /// <returns>
        /// A 200 OK status code along with the list of groups if the operation is successful;
        /// otherwise, a 400 Bad Request status code with an error message, or a 500 Internal Server Error status code if something goes wrong.
        /// </returns>
        /// <param name="addKeycloakSettings">The body of the configuration.</param>
        /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken"/> used to observe cancellation requests for the operation.</param>
        [HttpPost("new-realm")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfigureNewRealm([FromBody] AddKeycloakSettingsRequest addKeycloakSettings, CancellationToken cancellationToken)
        {
            return await AddOrUpdateClientConfigs(addKeycloakSettings, true, cancellationToken);
        }

        private async Task<IActionResult> AddOrUpdateClientConfigs(AddKeycloakSettingsRequest addKeycloakSettings, bool includeRealm, CancellationToken cancellationToken)
        {
            try
            {
                await commandMediator.SendAsync(new AddOrUpdateConfigCommand(addKeycloakSettings.ToMapper()), cancellationToken);

                if (includeRealm)
                {
                    var addRealmRequest = new AddRealmRequest(addKeycloakSettings.Realm.Name!.ToLower(), "");
                    var realmResult = await commandMediator.SendAsync(new AddRealmsCommand([addRealmRequest]), cancellationToken);
                    if (realmResult.IsFailure)
                    {
                        return BadRequest("Failed to create realm.");
                    }
                }

                var clientAndClientScopeResult = await HandleClientScopesRolesGroupRole(addKeycloakSettings, cancellationToken);
                if (clientAndClientScopeResult.IsFailure)
                {
                    return BadRequest("Failed when tried added basic configurations.");
                }

                return Created("/api/v1/config", "Initial configs created successfully!");
            }
            catch
            {
                await commandMediator.SendAsync(new DeleteRealmCommand(addKeycloakSettings.Realm.Name!), cancellationToken);
                throw;
            }

        }

        /// <summary>
        /// This method is creating all basic configuration to Keycloak works, it is creating Client, client scope, adding client scope to client, creating group and client role
        /// </summary>
        /// <param name="keyCloakSettings"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<Result> HandleClientScopesRolesGroupRole(AddKeycloakSettingsRequest keyCloakSettings, CancellationToken cancellationToken)
        {
            var clientBody = new AddClientRequest
            {
                ClientId = keyCloakSettings.Client.ClientId,
                Description = "This client is related to Feijuca.Api, this client will handle token generation and keycloak actions.",
                Urls = [$"{Request.Scheme}://{Request.Host}", $"{Request.Scheme}s://{Request.Host}"]
            };

            var addClientScopes = new List<AddClientScopesRequest>
            {
                new(Constants.FeijucaApiClientName, Constants.FeijucaApiClientName, true)
            };

            var result = await ProcessActionsAsync(
                async () => await commandMediator.SendAsync(new AddClientCommand(clientBody), cancellationToken),
                async () => await commandMediator.SendAsync(new AddClientScopesCommand(addClientScopes), cancellationToken));

            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }

            var clientScopes = await queryMediator.QueryAsync(new GetClientScopesQuery(), cancellationToken);
            var clients = await queryMediator.QueryAsync(new GetAllClientsQuery(), cancellationToken);
            var clientScope = clientScopes.FirstOrDefault(x => x.Name == Constants.FeijucaApiClientName)!;
            var feijucaClient = clients.FirstOrDefault(x => x.ClientId == Constants.FeijucaApiClientName)!;

            var addClientScopeToClientRequest = new AddClientScopeToClientRequest(feijucaClient.Id, clientScope.Id, false);
            var groupRequest = new AddGroupRequest(Constants.FeijucaGroupName, []);

            var addRolesRequest = new AddClientRolesRequest(
                [
                new(feijucaClient.Id, Constants.FeijucaRoleReadName, "Role related to the action to read data on the realm."),
                new(feijucaClient.Id, Constants.FeijucaRoleWriterName, "Role related to the action to write data on the realm.")
                ],
                false);

            var result2 = await ProcessActionsAsync(
                async () => await commandMediator.SendAsync(new AddClientScopeToClientCommand(addClientScopeToClientRequest), cancellationToken),
                async () => await commandMediator.SendAsync(new AddGroupCommand(groupRequest), cancellationToken),
                async () => await commandMediator.SendAsync(new AddClientRoleCommand(addRolesRequest), cancellationToken),
                async () => await commandMediator.SendAsync(new AddClientScopeAudienceProtocolMapperCommand(clientScope.Id), cancellationToken),
                async () => await commandMediator.SendAsync(new AddClientScopeMapperCommand(clientScopes.FirstOrDefault(x => x.Name == "profile")!.Id, "tenant", "tenant"), cancellationToken));

            if (result2.IsFailure)
            {
                return Result.Failure(result2.Error);
            }

            var clientRoles = await queryMediator.QueryAsync(new GetClientRolesQuery(), cancellationToken);
            var groups = await queryMediator.QueryAsync(new GetAllGroupsQuery(false), cancellationToken);

            var feijucaGroup = groups.Data.FirstOrDefault(x => x.Name == Constants.FeijucaGroupName);
            var feijucaRoles = clientRoles.Data.FirstOrDefault(x => x.Id == feijucaClient.Id)!.Roles;

            foreach (var roleName in new[] { Constants.FeijucaRoleReadName, Constants.FeijucaRoleWriterName })
            {
                var roleId = feijucaRoles.First(x => x.Name == roleName).Id;
                var clientRole = new AddClientRoleToGroupRequest(feijucaClient.Id, roleId);
                await commandMediator.SendAsync(new AddClientRoleToGroupCommand(feijucaGroup!.Id.ToString(), clientRole), cancellationToken);
            }

            var addUserRequest = new AddUserRequest(
                keyCloakSettings.RealmAdminUser.Username,
                keyCloakSettings.RealmAdminUser.Password,
                keyCloakSettings.RealmAdminUser.Username,
                "Admin",
                "Admin",
                []);

            var userId = await commandMediator.SendAsync(new AddUserCommand(keyCloakSettings.Realm.Name ?? "No Tenant", addUserRequest), cancellationToken);

            await commandMediator.SendAsync(new AddUserToGroupCommand(userId.Data, Guid.Parse(feijucaGroup!.Id)), cancellationToken);

            return Result.Success();
        }

        private static async Task<Result> ProcessActionsAsync(params Func<Task<Result>>[] actions)
        {
            foreach (var action in actions)
            {
                var result = await action();
                if (result.IsFailure)
                {
                    return result;
                }
            }
            return Result.Success();
        }
    }
}
