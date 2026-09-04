using Feijuca.Auth.Application.Requests.Realm;
using Feijuca.Auth.Application.Responses;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Http.Requests;
using Flurl;

namespace Feijuca.Auth.Application.Mappers
{
    public static class RealmMapper
    {
        public static RealmEntity ToRealmEntity(this AddRealmRequest addRealmRequest)
        {
            return new RealmEntity
            {
                Realm = addRealmRequest.Name,
                DisplayName = addRealmRequest.Description,
                Enabled = true,
                Attributes = [],
                BrowserSecurityHeaders = [],
                AccessTokenLifespan = (int)TimeSpan.FromHours(1).TotalSeconds,
                AccessTokenLifespanForImplicitFlow = (int)TimeSpan.FromMinutes(15).TotalSeconds,
                SsoSessionIdleTimeout = (int)TimeSpan.FromMinutes(30).TotalSeconds,
                SsoSessionMaxLifespan = (int)TimeSpan.FromHours(10).TotalSeconds,
                OfflineSessionIdleTimeout = (int)TimeSpan.FromDays(30).TotalSeconds,
                OfflineSessionMaxLifespan = (int)TimeSpan.FromDays(60).TotalSeconds
            };
        }

        public static IEnumerable<RealmResponse> ToResponse(this IEnumerable<RealmEntity> results, string issuer)
        {
            return results
                .Select(r => new RealmResponse(
                    issuer.AppendPathSegment("realms").AppendPathSegment(r.Realm),
                    r.Realm,
                    r.DisplayName ?? string.Empty,
                    r.Enabled,
                    r.Attributes
                    ));
        }

        public static RealmEntity ToUpdateSettings(this RealmEntity realm, UpdateRealmSettingsRequest request)
        {
            realm.AccessTokenLifespan = request.AccessTokenLifespan ?? realm.AccessTokenLifespan;
            realm.AccessTokenLifespanForImplicitFlow = request.AccessTokenLifespanForImplicitFlow ?? realm.AccessTokenLifespanForImplicitFlow;
            realm.SsoSessionIdleTimeout = request.SsoSessionIdleTimeout ?? realm.SsoSessionIdleTimeout;
            realm.SsoSessionMaxLifespan = request.SsoSessionMaxLifespan ?? realm.SsoSessionMaxLifespan;
            realm.OfflineSessionIdleTimeout = request.OfflineSessionIdleTimeout ?? realm.OfflineSessionIdleTimeout;
            realm.OfflineSessionMaxLifespan = request.OfflineSessionMaxLifespan ?? realm.OfflineSessionMaxLifespan;
            return realm;
        }
    }
}