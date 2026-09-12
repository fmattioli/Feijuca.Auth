namespace Feijuca.Auth.Http.Requests;

public record ReplicationConfigurationRequest(
    bool IncludeClients,
    bool IncludeClientRoles,
    bool IncludeClientScopes,
    bool CreateAdminGroupWithAllRulesAssociated,
    IncludeGroupsRequest IncludeGroups,
    LoginUserRequest AdminUser
    );
