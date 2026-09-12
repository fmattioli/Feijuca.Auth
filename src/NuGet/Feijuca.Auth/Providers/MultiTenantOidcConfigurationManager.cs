using Feijuca.Auth.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Feijuca.Auth.Authentication;

public sealed class MultiTenantOidcConfigurationManager(IHttpContextAccessor httpContextAccessor, IOidcConfigManagerCache cache) : IConfigurationManager<OpenIdConnectConfiguration>
{
    public const string IssuerKey = "Feijuca.Auth.Issuer";

    public Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancellationToken)
    {
        var manager = GetCurrentManager();

        return manager.GetConfigurationAsync(cancellationToken);
    }

    public void RequestRefresh()
    {
        var manager = GetCurrentManager();

        manager.RequestRefresh();
    }

    private IConfigurationManager<OpenIdConnectConfiguration> GetCurrentManager()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available.");

        if (!httpContext.Items.TryGetValue(IssuerKey, out var value) ||
            value is not string issuer)
        {
            throw new InvalidOperationException(
                "Issuer was not resolved for current request.");
        }

        var metadataAddress =
            $"{issuer.TrimEnd('/')}/.well-known/openid-configuration";

        return cache.Get(metadataAddress, requireHttps: false);
    }
}