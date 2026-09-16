namespace Feijuca.Auth.Domain.Entities
{
    public class ClientScopeEntity
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Protocol { get; set; }
        public Dictionary<string, string>? Attributes { get; set; }
        public List<ProtocolMapperEntity>? ProtocolMappers { get; set; }
    }

    public class ProtocolMapperEntity
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Protocol { get; set; }
        public required string ProtocolMapper { get; set; }
        public bool ConsentRequired { get; set; }
        public required Dictionary<string, string> Config { get; set; }
    }
}
