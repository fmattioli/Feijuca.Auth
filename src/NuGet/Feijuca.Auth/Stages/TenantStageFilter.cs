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
            ? MatchByVisibleToTenants(requestedTenant.Name)
            : MatchByTenant(tenantProvider.GetTenant().Name);
    }

    public static FilterDefinition<BsonDocument> MatchByTenant(string tenant)
    {
        return Builders<BsonDocument>.Filter.Eq(
            "Tenant",
            tenant);
    }

    public static FilterDefinition<BsonDocument> MatchByVisibleToTenants(string tenant)
    {
        return Builders<BsonDocument>.Filter.AnyEq(
            "VisibleToTenants",
            tenant);
    }
}