using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public record LoginCommand(string Tenant, LoginUserRequest LoginUser) : ICommand<Result<TokenDetailsResponse>>;
}
