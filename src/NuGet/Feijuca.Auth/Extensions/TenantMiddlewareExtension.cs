using Feijuca.Auth.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace Feijuca.Auth.Extensions;

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}