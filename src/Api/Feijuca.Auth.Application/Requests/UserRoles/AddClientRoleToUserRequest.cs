namespace Feijuca.Auth.Application.Requests.UserRoles;

public record AddClientRoleToUserRequest(string ClientId, Guid RoleId);