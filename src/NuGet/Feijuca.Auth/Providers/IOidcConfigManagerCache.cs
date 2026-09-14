using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Feijuca.Auth.Providers;

public interface IOidcConfigManagerCache
{
    IConfigurationManager<OpenIdConnectConfiguration> Get(string metadataAddress, bool requireHttps);
}
