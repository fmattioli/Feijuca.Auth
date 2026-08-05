namespace Feijuca.Auth.Application.Requests.UserRoles;

public record RemoveClientRoleFromUserRequest(string ClientId, Guid RoleId);
