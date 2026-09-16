using Feijuca.Auth.Models;
using LiteBus.Queries.Abstractions;

namespace Feijuca.Auth.Application.Queries.RealmAssociations;

public record GetRealmAssociationsQuery() : IQuery<Result<IEnumerable<string>>>;