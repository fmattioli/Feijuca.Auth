using Feijuca.Auth.Models;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace Feijuca.Auth.Providers;

public class TenantProvider(IHttpContextAccessor httpContextAccessor, JwtSecurityTokenHandler jwtSecurityTokenHandler) : ITenantProvider
{
    private User _userId = null!;

    private Tenant _tenant = null!;

    private Tenant _requestedTenant = null!;

    private IEnumerable<Tenant> _tenants = [];

    public User User => _userId;

    public Tenant Tenant => _tenant;

    public Tenant RequestedTenant => _requestedTenant;

    public IEnumerable<Tenant> Tenants => _tenants;

    public string GetInfo(string infoName)
    {
        string jwtToken = GetToken();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            var tokenInfos = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
            var userClaim = tokenInfos.Claims.FirstOrDefault(c => c.Type == infoName)?.Value!;
            return userClaim;
        }

        return string.Empty;
    }

    public IEnumerable<string> GetGroupNames()
    {
        string jwtToken = GetToken();

        if (!string.IsNullOrEmpty(jwtToken))
        {
            var tokenInfos = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
            var userClaims = tokenInfos.Claims.Where(c => c.Type == "groups").Select(c => c.Value.Trim('/'));
            return userClaims;
        }

        return [];
    }

    public User GetUser()
    {
        string jwtToken = GetToken();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            var tokenInfos = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
            var userId = tokenInfos.Claims.FirstOrDefault(c => c.Type == "sub")?.Value!;
            var userName = tokenInfos.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value!;
            return new User(Guid.Parse(userId), userName);
        }

        return new User(Guid.Empty, string.Empty);
    }

    public string GetToken()
    {
        var authorizationHeader = httpContextAccessor.HttpContext?.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
        {
            return string.Empty;
        }

        return authorizationHeader["Bearer ".Length..];
    }

    public void SetRequestedTenant(string tenant)
    {
        _requestedTenant = new Tenant(tenant);
    }

    public void SetTenants(IEnumerable<Tenant> tenants)
    {
        _tenants = tenants;
        _tenant = tenants.First();
    }
    
    public void SetTenant(Tenant tenant)
    {
        _tenant = tenant;
    }

    public void SetUser(User user)
    {
        _userId = user;
    }

    public Tenant GetTenant()
    {
        string jwtToken = GetToken();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            var tokenInfos = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
            var tenantClaim = tokenInfos.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value!;
            return new Tenant(tenantClaim);
        }

        return new Tenant(string.Empty);
    }


    public Tenant? GetRequestedTenant()
    {
        return _requestedTenant;
    }

    public Tenant GetTenantContext()
    {
        if (!string.IsNullOrWhiteSpace(_requestedTenant?.Name))
        {
            return _requestedTenant;
        }

        return _tenant;
    }

    public IEnumerable<Tenant> GetTenants()
    {
        string jwtToken = GetToken();
        if (!string.IsNullOrEmpty(jwtToken))
        {
            var tokenInfos = jwtSecurityTokenHandler.ReadJwtToken(jwtToken);
            var tenantClaim = tokenInfos.Claims.FirstOrDefault(c => c.Type == "tenant")?.Value!;

            var tenants = tenantClaim.Split(',').SelectMany(x =>
            {
                return new List<Tenant> { new(x) };
            });

            return tenants;
        }

        return [];
    }
}