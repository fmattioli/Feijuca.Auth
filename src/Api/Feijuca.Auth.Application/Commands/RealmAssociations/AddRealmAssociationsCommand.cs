using Feijuca.Auth.Application.Requests.RealmAssociations;
using Feijuca.Auth.Models;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.RealmAssociations;

public record AddRealmAssociationsCommand(AddRealmAssociationsRequest AddRealmAssociationsRequest) : ICommand<Result>;