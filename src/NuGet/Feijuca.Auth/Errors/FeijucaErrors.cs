using Feijuca.Auth.Models;

namespace Feijuca.Auth.Errors;

public static class FeijucaErrors
{
    public static readonly Error GetUserErrors = new("ErrorGetUsers", "An error occured while tried get users.");
    public static readonly Error GetGroupErrors = new("ErrorGetGroups", "An error occured while tried get groups.");
    public static readonly Error GetGroupUsersErrors = new("ErrorGetGroupUsers", "An error occured while tried get group users.");
    public static readonly Error GetRealmErrors = new("ErrorGetRealms", "An error occured while tried get realms.");
    public static readonly Error GenerateTokenError = new("GenerateTokenError", "An error occured while generate JWT Token.");
    public static readonly Error CreateGroupError = new("CreateGroupError", "An error occured while create new group");
    public static readonly Error CreateUserError = new("CreateUserError", "An error occured while create new group");
    public static readonly Error AddUserToGroupError = new("AddUserToGroupError", "An error occured while to add user in group");
    public static readonly Error UpdateGroupNameError = new("UpdateGroupNameError", "An error occured while updating group name.");
    public static readonly Error ReplicateRealmError = new("ReplicateRealmError", "An error occured while replicating realm.");
    public static readonly Error RemoveUsersFromGroupError = new("RemoveUsersFromGroupError", "An error occurred while removing users from group.");
    public static readonly Error CreateRealmError = new("CreateRealmError", "An error occurred while creating realm.");
}