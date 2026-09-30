using Feijuca.Auth.Application.Requests.Pagination;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Application.Requests.User
{
    public class GetUsersRequest
    {
        public GetUsersRequest()
        {
            PageFilter = new PageFilterRequest { Page = 1, PageSize = 60, };
        }

        [FromQuery]
        public PageFilterRequest PageFilter { get; set; }

        [FromQuery]
        public IEnumerable<Guid>? Ids { get; set; }

        [FromQuery]
        public IEnumerable<string>? Usernames { get; set; }

        [FromQuery]
        public bool? IsEnabled { get; set; }

        [FromQuery]
        public IEnumerable<string>? AttributeKeys { get; set; }

        [FromQuery]
        public IEnumerable<string>? AttributeValues { get; set; }
    }

    public record FilterAttribute(string Name, string Value);
}
