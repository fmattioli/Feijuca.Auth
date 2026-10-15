namespace Feijuca.Auth.Application.Requests.Role;

public record RegisterClientRoleRequest(IEnumerable<AddClientRoleRequest> ClientRoles);
