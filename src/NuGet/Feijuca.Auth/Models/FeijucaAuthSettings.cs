﻿namespace Feijuca.Auth.Models
{
    public class FeijucaAuthSettings
    {
        public required Client Client { get; set; }
        public required Secrets? Secrets { get; set; }
        public required ServerSettings ServerSettings { get; set; }
        public required IEnumerable<Realm> Realms { get; set; }
        public required ClientScopes? ClientScopes { get; set; }
    }
}
