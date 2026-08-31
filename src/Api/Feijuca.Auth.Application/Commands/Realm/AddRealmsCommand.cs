using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Realm
{
    public record AddRealmsCommand(IEnumerable<AddRealmRequest> AddRealmsRequest) : ICommand<Result<bool>>;
}
