using Feijuca.Auth.Models;

namespace Feijuca.Auth.Providers;

public interface ITenantProvider
{
    IEnumerable<Tenant> Tenants { get; }

    Tenant Tenant { get; }

    User User { get; }

    string GetInfo(string infoName);

    IEnumerable<string> GetGroupNames();

    IEnumerable<Tenant> GetTenants();

    Tenant GetTenant();

    Tenant? GetRequestedTenant();

    Tenant GetTenantContext();

    void SetTenant(string tenant);

    void SetRequestedTenant(string tenant);

    User GetUser();

    string GetToken();

    void SetTenants(IEnumerable<Tenant> tenants);

    void SetUser(User user);
}
