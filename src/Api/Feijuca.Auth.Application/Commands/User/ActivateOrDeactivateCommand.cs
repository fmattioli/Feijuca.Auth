using Feijuca.Auth.Application.Requests.User;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public record ActivateOrDeactivateCommand(Guid Id, ActivateOrDeactivateRequest Request) : ICommand<Result<bool>>;
}