using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Client;

public record SyncClientCommand(string TargetTenant) : ICommand<Result>;