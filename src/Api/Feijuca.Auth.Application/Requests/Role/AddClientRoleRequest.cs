namespace Feijuca.Auth.Application.Requests.Role
{
    public record AddClientRoleRequest(string ClientId, string Name, string Description);
}