using Feijuca.Auth.Application.Requests.Client;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Client;

public record SyncClientCommand(SyncClientRequest SyncClientRequest) : ICommand<Result>;