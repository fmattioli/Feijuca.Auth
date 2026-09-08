using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Http.Requests;

namespace Feijuca.Auth.Application.Mappers
{
    public static class GroupMapper
    {
        public static IEnumerable<GroupResponse> ToResponse(this IEnumerable<Group> group)
        {
            return group.Select(x => new GroupResponse(x.Id, x.Name, x.Path));
        }

        public static GroupResponse ToResponse(this Group group)
        {
            return new GroupResponse(group.Id, group.Name, group.Path);
        }

        public static PagedResult<UserGroupResponse> ToResponse(this UserGroupResponse results, PageFilterRequest pageFilter, int totalResults)
        {
            return new PagedResult<UserGroupResponse>
            {
                PageNumber = pageFilter.Page,
                PageSize = pageFilter.PageSize,
                Results = [results],
                TotalResults = totalResults
            };
        }
    }
}
