using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Queries.Realm
{
    public record ReplicateRealmCommand(ReplicateRealmRequest ReplicateRealmRequest) : ICommand<Result<bool>>;
}