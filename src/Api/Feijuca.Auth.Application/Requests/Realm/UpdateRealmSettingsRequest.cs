namespace Feijuca.Auth.Application.Requests.Realm;

public record UpdateRealmSettingsRequest(
    int? AccessTokenLifespanFromHours,
    int? AccessTokenLifespanForImplicitFlowFromMinutes,
    int? SsoSessionIdleTimeoutFromMinutes,
    int? SsoSessionMaxLifespanFromHours,
    int? OfflineSessionIdleTimeoutFromDays,
    int? OfflineSessionMaxLifespanFromDays);