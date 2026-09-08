using Feijuca.Auth.Application.Requests.GroupUsers;
using Feijuca.Auth.Application.Requests.User;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Filters;
using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Application.Mappers
{
    public static class UserMapper
    {
        public static Domain.Entities.User ToDomain(this AddUserRequest userRequest, string tenant)
        {
            var atributtes = new Dictionary<string, string[]>
            {
                { "Tenant", [tenant] }
            };

            foreach (var item in userRequest.Attributes?.Where(x => x.Key != "Tenant") ?? [])
            {
                atributtes.Add(item.Key, item.Value);
            }

            return new Domain.Entities.User(userRequest.Username, userRequest.Password, userRequest.Email!, userRequest.FirstName!, userRequest.LastName!, atributtes);
        }

        public static IEnumerable<UserResponse> ToUsersResponse(this IEnumerable<Domain.Entities.User> users, string tenant)
        {
            return users.Select(x => new UserResponse(x.Id,
                x.Enabled,
                x.EmailVerified,
                x.Username,
                x.Email,
                x.FirstName ?? "",
                x.LastName!,
                tenant,
                x.Totp,
                x.DisableableCredentialTypes,
                x.RequiredActions,
                x.NotBefore,
                x.CreatedTimestamp,
                x.Access?.ToAcess(),
                x.Attributes));
        }

        public static Http.Responses.Access ToAcess(this Domain.Entities.Access access)
        {
            return new Http.Responses.Access
            {
                Impersonate = access.Impersonate,
                Manage = access.Manage,
                ManageGroupMembership = access.ManageGroupMembership,
                MapRoles = access.MapRoles,
                View = access.View,
            };
        }

        public static Domain.Entities.User ToLoginUserDomain(this LoginUserRequest loginUserRequest)
        {
            return new Domain.Entities.User(loginUserRequest.Username, loginUserRequest.Password);
        }

        public static Result<TokenDetailsResponse> ToTokenResponse(this Result<TokenDetails> tokenDetails)
        {
            var tokenDetailsResponse = new TokenDetailsResponse(
                tokenDetails.Data.Access_Token,
                tokenDetails.Data.Expires_In,
                tokenDetails.Data.Refresh_Expires_In,
                tokenDetails.Data.Refresh_Token,
                tokenDetails.Data.Token_Type,
                tokenDetails.Data.Scope);

            return Result<TokenDetailsResponse>.Success(tokenDetailsResponse);
        }

        public static UserFilters ToUserFilters(this GetUsersRequest getUsersRequest)
        {
            var pageFilter = new PageFilter(getUsersRequest.PageFilter.Page, getUsersRequest.PageFilter.PageSize);
            return new UserFilters(pageFilter, getUsersRequest.Ids, getUsersRequest.Usernames);
        }

        public static UserFilters ToUserFilters(this GetUsersGroupRequest getUsersRequest)
        {
            var pageFilter = new PageFilter(getUsersRequest.PageFilter.Page, getUsersRequest.PageFilter.PageSize);
            return new UserFilters(pageFilter, [], getUsersRequest.Usernames);
        }

        public static Responses.PagedResult<UserResponse> ToUserResponse(this IEnumerable<Domain.Entities.User> results, PageFilterRequest pageFilter, string tenant, int totalResults)
        {
            return new Responses.PagedResult<UserResponse>
            {
                PageNumber = pageFilter.Page,
                PageSize = pageFilter.PageSize,
                Results = results.Select(x => x.ToResponse(tenant)),
                TotalResults = totalResults,
            };
        }

        public static UserResponse ToResponse(this Domain.Entities.User user, string tenant)
        {
            return new UserResponse(user.Id,
                user.Enabled,
                user.EmailVerified,
                user.Username,
                user.Email,
                user.FirstName ?? "",
                user.LastName!,
                tenant,
                user.Totp,
                user.DisableableCredentialTypes,
                user.RequiredActions,
                user.NotBefore,
                user.CreatedTimestamp,
                user.Access?.ToAcess(),
                user.Attributes);
        }
    }
}
