using Feijuca.Auth.Http.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Feijuca.Auth.Application.Requests.GroupUsers
{
    public class GetUsersGroupRequest
    {
        public GetUsersGroupRequest()
        {
            PageFilter = new PageFilterRequest { Page = 1, PageSize = 60, };
        }

        [FromQuery]
        public PageFilterRequest PageFilter { get; set; }

        [FromQuery]
        public string GroupId { get; set; } = null!;

        [FromQuery]
        public IEnumerable<string>? Usernames { get; set; }
    }
}
