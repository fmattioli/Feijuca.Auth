using Feijuca.Auth.Common.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace Feijuca.Auth.Infra.CrossCutting.Extensions
{
    public static class HealthCheckersExtensions
    {
        public static IServiceCollection AddHealthCheckers(this IServiceCollection services, KeycloakSettings keycloakSettings)
        {
            if (keycloakSettings != null)
            {
                var baseUri = new Uri(keycloakSettings.ServerSettings.Url, UriKind.Absolute);
                var keycloakHealthUri = new Uri(baseUri, "health/ready");

                services.AddHealthChecks()
                    .AddMongoDb(
                        clientFactory: sp => sp.GetRequiredService<MongoDB.Driver.IMongoClient>(),
                        name: "mongoDB",
                        tags: ["db", "mongo"]);
                    // .AddUrlGroup(
                    //    keycloakHealthUri,
                    //    name: "keycloak",
                    //    tags: ["keycloak", "auth"]);
            }

            return services;
        }

        public static void UseHealthCheckers(this IApplicationBuilder app, KeycloakSettings keycloakSettings)
        {
            if (keycloakSettings != null)
            {
                app.UseHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            }
        }
    }
}