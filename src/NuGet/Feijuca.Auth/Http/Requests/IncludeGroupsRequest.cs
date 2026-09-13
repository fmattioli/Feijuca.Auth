namespace Feijuca.Auth.Http.Requests;

public record IncludeGroupsRequest(bool Include, IEnumerable<string>? GroupNames);