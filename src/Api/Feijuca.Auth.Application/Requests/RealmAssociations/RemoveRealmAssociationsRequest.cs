namespace Feijuca.Auth.Application.Requests.RealmAssociations;

public record RemoveRealmAssociationsRequest(string TargetRealm, IEnumerable<string> RealmsToDisassociate);