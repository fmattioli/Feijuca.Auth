using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.ClientRole;

public record SyncClientRoleCommand(string TargetTenant) : ICommand<Result<bool>>;