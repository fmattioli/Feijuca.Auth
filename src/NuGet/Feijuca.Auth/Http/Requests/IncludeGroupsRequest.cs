namespace Feijuca.Auth.Http.Requests;

public record IncludeGroupsRequest(bool IncludeGroups, IEnumerable<string>? Groups);