namespace Feijuca.Auth.Http.Requests;

public record ReplicationConfigurationRequest(
    bool IncludeClients,
    bool IncludeClientRoles,
    bool IncludeClientScopes,
    bool CreateAdminGroupWithAllRulesAssociated,
    string[] IncludeGroups,
    LoginUserRequest AdminUser
    );