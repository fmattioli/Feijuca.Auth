namespace Feijuca.Auth.Application.Requests.Client
{
    public class AddClientRequest()
    {
        public required string ClientId { get; set; }
        public required string Description { get; set; }
        public required IEnumerable<string> Urls { get; set; }
        public bool AllTenants { get; set; }
    }
}