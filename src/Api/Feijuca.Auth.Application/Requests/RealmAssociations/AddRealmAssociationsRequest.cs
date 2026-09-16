namespace Feijuca.Auth.Application.Requests.RealmAssociations;

public record AddRealmAssociationsRequest(string TargetRealm, IEnumerable<string> RealmsToAssociate);