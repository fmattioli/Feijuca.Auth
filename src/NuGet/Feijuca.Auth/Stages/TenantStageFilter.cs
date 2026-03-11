using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.RegularExpressions;

namespace Feijuca.Auth.Stages;

public static class TenantStageFilter
{
    public static FilterDefinition<BsonDocument> MatchByTenants(IEnumerable<string> tenantNames)
    {
        var names = tenantNames.Select(x => new BsonRegularExpression(new Regex(x, RegexOptions.IgnoreCase)));

        var filter = new BsonDocument(
            "Tenants",
            new BsonDocument("$in", BsonArray.Create(names)));

        return new BsonDocumentFilterDefinition<BsonDocument>(filter);
    }

    public static FilterDefinition<BsonDocument> MatchByTenant(string tenant)
    {
        var filter = new BsonDocument(
            "Tenant",
            new BsonDocument("$eq", tenant));

        return new BsonDocumentFilterDefinition<BsonDocument>(filter);
    }
}
