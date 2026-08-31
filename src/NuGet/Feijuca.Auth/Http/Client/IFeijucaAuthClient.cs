using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Http.Requests;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Http.Client;

public interface IFeijucaAuthClient
{
    Task<Result<TokenDetailsResponse>> AuthenticateUserAsync(string tenant, string username, string password, CancellationToken cancellationToken);
    Task<Result<PagedResult<UserResponse>>> GetUsersAsync(string tenant, int maxUsers, string jwtToken, CancellationToken cancellationToken);
    Task<Result<UserResponse>> GetUserAsync(string tenant, string userame, string jwtToken, CancellationToken cancellationToken);
    Task<Result<IEnumerable<GroupResponse>>> GetGroupsAsync(string tenant, string jwtToken, CancellationToken cancellationToken);
    Task<Result<PagedResult<UserGroupResponse>>> GetGroupUsersAsync(string groupId, string jwtToken, CancellationToken cancellationToken);
    Task<Result> RemoveUsersFromGroupAsync(Guid UserId, Guid GroupId, CancellationToken cancellationToken);
    Task<Result<IEnumerable<RealmResponse>>> GetRealmsAsync(string jwtToken, CancellationToken cancellationToken);
    Task<Result<string>> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken);
    Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, string tenant, CancellationToken cancellationToken);
    Task<Result> AddUserToGroupAsync(AddUserToGroupRequest request, CancellationToken cancellationToken);
    Task<Result> ReplicateRealmAsync(ReplicateRealmRequest request, CancellationToken cancellationToken);
    Task<Result> CreateRealmAsync(IEnumerable<AddRealmRequest> AddRealmsRequest, CancellationToken cancellationToken);
    Task<Result> UpdateGroupNameAsync(Guid groupId, UpdateGroupNameRequest request, CancellationToken cancellationToken);
    void SetToken(string token);
}