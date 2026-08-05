namespace Feijuca.Auth.Application.Responses;

public record UserRolesResponse(string Id, string Client, IEnumerable<RoleResponse> Mappings);