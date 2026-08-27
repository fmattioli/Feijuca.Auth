namespace Feijuca.Auth.Application.Requests.Role;

public record AddClientRolesRequest(IEnumerable<AddClientRoleRequest> ClientRoles, bool AllTenants, string? TargetTenant);