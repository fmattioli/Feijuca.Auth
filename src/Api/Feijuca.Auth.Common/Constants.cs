namespace Feijuca.Auth.Common;

public static class Constants
{
    public const string FeijucaApiClientName = "feijuca-auth-api";
    public const string FeijucaGroupName = "feijuca-auth-api";
    public const string FeijucaRoleWriterName = "Feijuca.ApiWriter";
    public const string FeijucaRoleReadName = "Feijuca.ApiReader";
    public const string AdminGroupName = "Admins";
    public const string AllowedTenantsProtocolMapperName = "allowed-tenants";
    public const string AllowedTenantsConfigKey = "claim.value";

    public static readonly IReadOnlyDictionary<string, string> ReplicableRealmAttributes = new Dictionary<string, string>
    {
        ["enableReceiveOperations"] = "false"
    };
}
