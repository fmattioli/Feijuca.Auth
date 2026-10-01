using Feijuca.Auth.Application.Requests.User;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public record ActivateUserCommand(Guid Id, ActivateUserRequest Request) : ICommand<Result<bool>>;
}