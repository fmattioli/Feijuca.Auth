using Feijuca.Auth.Application.Requests.Role;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public record RegisterClientRoleCommand(RegisterClientRoleRequest Request) : ICommand<Result>;
