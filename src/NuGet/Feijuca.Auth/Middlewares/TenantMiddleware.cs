using Feijuca.Auth.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Feijuca.Auth.Middlewares;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantService)
    {
        // What tenant the action should be performed
        var tenantFromHeader = context.Request.Headers["Tenant"].FirstOrDefault();

        if (!string.IsNullOrEmpty(tenantFromHeader))
        {
            tenantService.SetRequestedTenant(tenantFromHeader);
        }

        var endpoint = context.GetEndpoint();

        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        var requiresAuthorization = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>().Any() == true;

        if (allowAnonymous || !requiresAuthorization)
        {
            await next(context);
            return;
        }

        var tenants = tenantService.GetTenants();
        var user = tenantService.GetUser();

        if (!tenants.Any() || user.Id == Guid.Empty)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Jwt token authorization header is required."
            });

            return;
        }

        // Who is authenticated following JWT token
        tenantService.SetTenants(tenants);
        tenantService.SetUser(user);

        tenantService.SetEffectiveTenant();

        await next(context);
    }
}
