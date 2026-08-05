using Feijuca.Auth.Application.Requests.UserRoles;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.UserRoles;

public record AddClientRoleToUserCommand(string UserId, AddClientRoleToUserRequest AddRoleToUserRequest) : ICommand<Result>;