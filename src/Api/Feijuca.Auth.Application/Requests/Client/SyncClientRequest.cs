namespace Feijuca.Auth.Application.Requests.Client;

public record SyncClientRequest(string? TargetTenant, bool AllTenants = false);