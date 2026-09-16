using Feijuca.Auth.Application.Requests.ClientScopes;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Application.Responses;

namespace Feijuca.Auth.Application.Mappers
{
    public static class ClientScopesMapper
    {
        public static ClientScopeEntity ToClientScopesEntity(this AddClientScopesRequest addClientScopesRequest)
        {
            return new ClientScopeEntity
            {
                Name = addClientScopesRequest.Name,
                Description = addClientScopesRequest.Description
            };
        }

        public static IEnumerable<ClientScopesResponse> ToClientScopesResponse(this IEnumerable<ClientScopeEntity> clientScopeEntities)
        {
            var list = new List<ClientScopesResponse>();

            foreach (var item in clientScopeEntities)
            {
                list.Add(new ClientScopesResponse(item.Id ?? "", 
                    item.Name ?? "", 
                    item.Description ?? "", 
                    item.Protocol ?? "", 
                    item.Attributes ?? [], 
                    item.ProtocolMappers?.ToProtocolMapperResponse() ?? []));
            }

            return list;
        }

        public static IEnumerable<ProtocolMapperResponse> ToProtocolMapperResponse(this IEnumerable<ProtocolMapperEntity> protocolMapperEntity)
        {
            var list = new List<ProtocolMapperResponse>();

            if (protocolMapperEntity != null)
            {
                foreach (var item in protocolMapperEntity)
                {
                    list.Add(new ProtocolMapperResponse(item.Id, item.Name, item.Protocol, item.ProtocolMapper, item.ConsentRequired, item.Config));
                }
            }

            return list;
        }
    }
}
