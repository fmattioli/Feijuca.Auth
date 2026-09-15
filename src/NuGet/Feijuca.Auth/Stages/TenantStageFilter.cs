using Feijuca.Auth.Providers;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Feijuca.Auth.Stages;

public static class TenantStageFilter
{
    public static FilterDefinition<BsonDocument> MatchByContext(ITenantProvider tenantProvider)
    {
        var requestedTenant = tenantProvider.GetRequestedTenant();

        return !string.IsNullOrWhiteSpace(requestedTenant?.Name)
            ? MatchByVisibleToTenants(requestedTenant.Name, tenantProvider.Tenant.Name)
            : MatchByTenant(tenantProvider.Tenant.Name);
    }

    public static FilterDefinition<BsonDocument> MatchByTenant(string tenant)
    {
        return Builders<BsonDocument>.Filter.Eq(
            "Tenant",
            tenant);
    }

    public static FilterDefinition<BsonDocument> MatchByVisibleToTenants(string requestedTenant, string visibleToTenant)
    {
        var requestTenantFilter = Builders<BsonDocument>.Filter.Eq(
            "Tenant",
            requestedTenant);

        var visibleToTenantsFilter = Builders<BsonDocument>.Filter.AnyEq(
            "VisibleToTenants",
            visibleToTenant);

        return Builders<BsonDocument>.Filter.And(
            requestTenantFilter,
            visibleToTenantsFilter);
    }
}