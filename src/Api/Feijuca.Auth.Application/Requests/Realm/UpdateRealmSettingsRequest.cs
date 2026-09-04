namespace Feijuca.Auth.Application.Requests.Realm;

public record UpdateRealmSettingsRequest(
    int? AccessTokenLifespan,
    int? AccessTokenLifespanForImplicitFlow,
    int? SsoSessionIdleTimeout,
    int? SsoSessionMaxLifespan,
    int? OfflineSessionIdleTimeout,
    int? OfflineSessionMaxLifespan);