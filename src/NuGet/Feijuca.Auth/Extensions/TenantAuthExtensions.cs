using Feijuca.Auth.Authentication;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Feijuca.Auth.Extensions;

public static class TenantAuthExtensions
{
    public static IServiceCollection AddApiAuthentication(this IServiceCollection services, FeijucaAuthConfiguration feijucaAuthConfiguration)
    {
        services.AddKeyCloakAuth(feijucaAuthConfiguration);

        return services;
    }

    public static IServiceCollection AddKeyCloakAuth(this IServiceCollection services, FeijucaAuthConfiguration feijucaAuthConfiguration)
    {
        var keycloakBaseUrl = feijucaAuthConfiguration.KeycloakUrl.TrimEnd('/');

        services
            .AddHttpContextAccessor()
            .AddSingleton<IOidcConfigManagerCache, OidcConfigManagerCache>()
            .AddSingleton<MultiTenantOidcConfigurationManager>()
            .AddSingleton<JwtSecurityTokenHandler>()
            .AddScoped<ITenantProvider, TenantProvider>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakWebApi(
                options =>
                {
                    options.Resource = "feijuca-auth-api";
                },
                options =>
                {
                    options.RequireHttpsMetadata = true;

                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidAudience = "feijuca-auth-api",
                            ClockSkew = TimeSpan.FromMinutes(2),

                            IssuerValidator = (
                                issuer,
                                securityToken,
                                validationParameters) =>
                            {
                                if (string.IsNullOrWhiteSpace(issuer) ||
                                    !IsValidIssuer(issuer, keycloakBaseUrl))
                                {
                                    throw new SecurityTokenInvalidIssuerException(
                                        "Invalid issuer");
                                }

                                return issuer;
                            }
                        };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived =
                            OnMessageReceived(keycloakBaseUrl),

                        OnAuthenticationFailed =
                            OnAuthenticationFailed,

                        OnChallenge =
                            OnChallenge
                    };
                });

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<MultiTenantOidcConfigurationManager>(
                (options, manager) =>
                {
                    options.ConfigurationManager = manager;
                });

        ConfigureAuthorization(services, []);

        return services;
    }

    private static Func<MessageReceivedContext, Task> OnMessageReceived(string keycloakBaseUrl)
    {
        return context =>
        {
            try
            {
                var rawAuthorization =
                    context.Request.Headers.Authorization.FirstOrDefault();

                var rawQueryToken =
                    context.Request.Query["access_token"].FirstOrDefault();

                string? token = null;

                if (!string.IsNullOrWhiteSpace(rawAuthorization))
                {
                    const string bearerPrefix = "Bearer ";

                    if (!rawAuthorization.StartsWith(
                            bearerPrefix,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        context.HttpContext.Items["AuthError"] = "Invalid authorization scheme";
                        context.Fail("Invalid authorization scheme");
                        return Task.CompletedTask;
                    }

                    token = rawAuthorization[bearerPrefix.Length..].Trim();
                }
                else if (!string.IsNullOrWhiteSpace(rawQueryToken))
                {
                    token = rawQueryToken.Trim();
                }

                if (string.IsNullOrWhiteSpace(token))
                {
                    context.HttpContext.Items["AuthError"] = "Missing token";
                    context.Fail("Missing token");
                    return Task.CompletedTask;
                }

                var handler = context.HttpContext.RequestServices
                    .GetRequiredService<JwtSecurityTokenHandler>();

                if (!handler.CanReadToken(token))
                {
                    context.HttpContext.Items["AuthError"] = "Invalid token format";
                    context.Fail("Invalid token format");
                    return Task.CompletedTask;
                }

                var jwt = handler.ReadJwtToken(token);
                var issuer = jwt.Issuer?.TrimEnd('/');

                if (string.IsNullOrWhiteSpace(issuer))
                {
                    context.HttpContext.Items["AuthError"] = "Missing issuer";
                    context.Fail("Missing issuer");
                    return Task.CompletedTask;
                }

                if (!IsValidIssuer(issuer, keycloakBaseUrl))
                {
                    context.HttpContext.Items["AuthError"] = "Invalid issuer";
                    context.Fail("Invalid issuer");
                    return Task.CompletedTask;
                }

                context.Token = token;

                context.HttpContext.Items[
                    MultiTenantOidcConfigurationManager.IssuerKey
                ] = issuer;

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                context.HttpContext.Items["AuthError"] = $"Authentication setup failed: {ex.Message}";
                context.HttpContext.Items["AuthStatusCode"] = 401;
                context.Fail($"Authentication setup failed: {ex.Message}");
                return Task.CompletedTask;
            }
        };
    }

    private static Task OnAuthenticationFailed(AuthenticationFailedContext context)
    {
        var errorMessage = context.HttpContext.Items["AuthError"] as string ?? context.Exception?.Message ?? "Authentication failed!";

        var statusCode = context.HttpContext.Items["AuthStatusCode"] as int? ?? StatusCodes.Status401Unauthorized;

        if (context.Response.HasStarted)
        {
            return Task.CompletedTask;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        return context.Response.WriteAsJsonAsync(
            new
            {
                error = errorMessage
            });
    }

    private static async Task OnChallenge(JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        if (context.Response.HasStarted)
            return;

        var errorMessage = context.HttpContext.Items["AuthError"] as string ?? context.ErrorDescription ?? "Authentication failed!";

        var statusCode = context.HttpContext.Items["AuthStatusCode"] as int? ?? StatusCodes.Status401Unauthorized;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            new
            {
                message = errorMessage
            });
    }

    private static void ConfigureAuthorization(IServiceCollection services, IEnumerable<Policy>? policySettings)
    {
        services
            .AddAuthorization()
            .AddKeycloakAuthorization();

        foreach (var policy in
                 (policySettings ?? [])
                 .Where(policy => !string.IsNullOrWhiteSpace(policy.Name)))
        {
            services
                .AddAuthorizationBuilder()
                .AddPolicy(
                    policy.Name,
                    authorizationPolicy =>
                    {
                        authorizationPolicy.RequireResourceRolesForClient(
                            "feijuca-auth-api",
                            [.. policy.Roles!]);
                    });
        }
    }

    private static bool IsValidIssuer(
        string issuer,
        string keycloakBaseUrl)
    {
        if (!Uri.TryCreate(
                issuer,
                UriKind.Absolute,
                out var issuerUri))
        {
            return false;
        }

        if (!Uri.TryCreate(
                keycloakBaseUrl,
                UriKind.Absolute,
                out var keycloakUri))
        {
            return false;
        }

        if (!string.Equals(
                issuerUri.Scheme,
                keycloakUri.Scheme,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(
                issuerUri.Authority,
                keycloakUri.Authority,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!issuerUri.AbsolutePath.StartsWith(
                "/realms/",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var realm =
            issuerUri.AbsolutePath["/realms/".Length..];

        if (string.IsNullOrWhiteSpace(realm) ||
            realm.Contains('/'))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(issuerUri.Query) ||
            !string.IsNullOrEmpty(issuerUri.Fragment))
        {
            return false;
        }

        return true;
    }
}