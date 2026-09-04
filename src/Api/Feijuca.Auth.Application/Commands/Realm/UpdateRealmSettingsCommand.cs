using Feijuca.Auth.Application.Requests.Realm;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.Realm;

public record UpdateRealmSettingsCommand(UpdateRealmSettingsRequest Request) : ICommand<Result>;