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
                BrowserSecurityHeaders = []
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
    }
}