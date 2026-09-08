namespace Feijuca.Auth.Domain.Entities
{
    public class RealmEntity
    {
        public required string Realm { get; set; }
        public string? DisplayName { get; set; }
        public bool Enabled { get; set; }
        public required Dictionary<string, string> Attributes { get; set; }
        public required Dictionary<string, string> BrowserSecurityHeaders { get; set; }
        public int AccessTokenLifespan { get; set; }
        public int AccessTokenLifespanForImplicitFlow { get; set; }
        public int SsoSessionIdleTimeout { get; set; }
        public int SsoSessionMaxLifespan { get; set; }
        public int OfflineSessionIdleTimeout { get; set; }
        public int OfflineSessionMaxLifespan { get; set; }
    }
}