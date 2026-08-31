using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.GroupRoles
{
    public record AddClientRoleToGroupCommand(string GroupId, AddClientRoleToGroupRequest AddRoleToGroupRequest) : ICommand<Result<bool>>;
}
