namespace Feijuca.Auth.Http.Requests;

public record RemoveUserFromGroupRequest(Guid UserId, Guid GroupId);