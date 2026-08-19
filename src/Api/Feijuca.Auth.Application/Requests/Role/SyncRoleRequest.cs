namespace Feijuca.Auth.Application.Requests.Role;

public record SyncRoleRequest(string? TargetTenant, bool AllTenants = false);