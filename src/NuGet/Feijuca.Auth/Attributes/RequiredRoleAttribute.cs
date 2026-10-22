using System.Text.Json;
using Feijuca.Auth.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Feijuca.Auth.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RequiredRoleAttribute(string role) : Attribute, IAuthorizationFilter
{
    private readonly string _role = role;

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        var realmRoles = user.FindFirst("realm_access")?.Value;

        if (!string.IsNullOrWhiteSpace(realmRoles))
        {
            using var document = JsonDocument.Parse(realmRoles);

            if (document.RootElement.TryGetProperty("roles", out var roles) && roles.EnumerateArray().Any(r => r.GetString() == _role))
            {
                return;
            }
        }

        var clientRoles = user.FindFirst("resource_access")?.Value;

        if (!string.IsNullOrWhiteSpace(clientRoles))
        {
            using var document = JsonDocument.Parse(clientRoles);

            if (document.RootElement.EnumerateObject().Any(client =>
                client.Value.TryGetProperty("roles", out var roles) && roles.EnumerateArray().Any(r => r.GetString() == _role)))
            {
                return;
            }
        }

        context.Result = new ObjectResult(FeijucaErrors.RequiredRoleError(_role))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}