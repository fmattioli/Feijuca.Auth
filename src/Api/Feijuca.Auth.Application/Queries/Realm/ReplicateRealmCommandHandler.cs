using Feijuca.Auth.Common;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;
using Feijuca.Auth.Models;

namespace Feijuca.Auth.Application.Queries.Realm
{
    public class ReplicateRealmCommandHandler(
        IUserRepository userRepository,
        IClientRepository clientRepository,
        IClientScopesRepository clientScopesRepository,
        IClientRoleRepository clientRoleRepository,
        IGroupRepository groupRepository,
        IGroupUsersRepository groupUsersRepository,
        IGroupRolesRepository groupRolesRepository,
        IRealmRepository realmRepository,
        ITenantProvider tenantProvider) : ICommandHandler<ReplicateRealmCommand, Result<bool>>
    {
        public async Task<Result<bool>> HandleAsync(ReplicateRealmCommand request, CancellationToken cancellationToken)
        {
            var targetTenant = request.ReplicateRealmRequest.Tenant;
            var originTenant = tenantProvider.Tenant.Name;

            var attributesReplicated = await ReplicateRealmAttributesAsync(targetTenant, cancellationToken);
            if (!attributesReplicated)
            {
                return Result<bool>.Failure(RealmErrors.ReplicateRealmError);
            }

            string adminGroupId = "";
            if (request.ReplicateRealmRequest!.ReplicationConfigurationRequest.AdminUser.Username != string.Empty)
            {
                adminGroupId = await CreateAdminGroupWithAllRulesAssociatedAsync(request, targetTenant, cancellationToken);
                var user = await CreateUserAsync(request, targetTenant, cancellationToken);
                await AssociateUserToTheGroupAsync(targetTenant, adminGroupId, user, cancellationToken);
            }

            var originClients = await clientRepository.GetClientsAsync(originTenant, cancellationToken);

            if (request.ReplicateRealmRequest.ReplicationConfigurationRequest.IncludeClients)
            {
                foreach (var client in originClients?.Data ?? [])
                {
                    var clientId = (await clientRepository.CreateClientAsync(client, targetTenant, cancellationToken)).Data;

                    if (request.ReplicateRealmRequest?.ReplicationConfigurationRequest.IncludeClientRoles ?? false)
                    {
                        await AssociatedRulesToTheClientAsync(targetTenant, originTenant, client, clientId, cancellationToken);

                        if (request.ReplicateRealmRequest?.ReplicationConfigurationRequest.CreateAdminGroupWithAllRulesAssociated ?? false)
                        {
                            await AssociateClientRulesToTheGroupAsync(targetTenant, adminGroupId, clientId, cancellationToken);
                        }
                    }
                }
            }

            if (request.ReplicateRealmRequest?.ReplicationConfigurationRequest.IncludeClientScopes ?? false)
            {
                var targetTenantClientsCreated = await clientRepository.GetClientsAsync(targetTenant, cancellationToken);

                foreach (var targetTenantClientCreated in targetTenantClientsCreated.Data)
                {
                    var originClientId = originClients!.Data.First(x => x.ClientId == targetTenantClientCreated.ClientId).Id;

                    var clientScopesAssociatedToTheClient = await clientScopesRepository.GetClientScopesAssociatedToTheClientAsync(originTenant,
                        originClientId,
                        cancellationToken);

                    foreach (var clientScopeAssociatedToTheClient in clientScopesAssociatedToTheClient)
                    {
                        var result = await clientScopesRepository.AddClientScopesAsync(clientScopeAssociatedToTheClient, targetTenant, cancellationToken);

                        if (string.IsNullOrEmpty(result))
                        {
                            continue;
                        }

                        await clientScopesRepository.AddClientScopeToClientAsync(targetTenantClientCreated.Id,
                            targetTenant,
                            result,
                            targetTenantClientCreated.ClientId != Constants.FeijucaApiClientName,
                            cancellationToken);

                        if (targetTenantClientCreated.ClientId == Constants.FeijucaApiClientName)
                        {
                            var targetClientScopes = await clientScopesRepository.GetClientScopesAsync(targetTenant, cancellationToken);
                            var clientScopeFeijuca = targetClientScopes.FirstOrDefault(x => x.Name == Constants.FeijucaApiClientName)!;
                            await clientScopesRepository.AddAudienceMapperAsync(clientScopeFeijuca.Id!, targetTenant, cancellationToken);
                            await clientScopesRepository.AddGroupMembershipMapperAsync(clientScopeFeijuca.Id!, targetTenant, cancellationToken);
                        }
                    }
                }

                var clientScopeProfile = await clientScopesRepository.GetClientScopeProfileAsync(targetTenant, cancellationToken);
                await clientScopesRepository.AddUserPropertyMapperAsync(clientScopeProfile.Id!, "tenant", "tenant", targetTenant, cancellationToken);
            }

            return Result<bool>.Success(true);
        }

        private async Task<bool> ReplicateRealmAttributesAsync(string targetTenant, CancellationToken cancellationToken)
        {
            var originRealm = await realmRepository.GetAsync(tenantProvider.Tenant.Name, cancellationToken);
            var targetRealm = await realmRepository.GetAsync(targetTenant, cancellationToken);
            if (!targetRealm.IsSuccess)
            {
                return false;
            }

            var attributes = targetRealm.Data.Attributes ?? new Dictionary<string, string>();
            foreach (var replicableAttribute in Constants.ReplicableRealmAttributes)
            {
                attributes[replicableAttribute.Key] = replicableAttribute.Value;
            }

            targetRealm.Data.Attributes = attributes;
            targetRealm.Data.AccessTokenLifespan = originRealm.Data.AccessTokenLifespan;
            targetRealm.Data.AccessTokenLifespanForImplicitFlow = originRealm.Data.AccessTokenLifespanForImplicitFlow;
            targetRealm.Data.SsoSessionIdleTimeout = originRealm.Data.SsoSessionIdleTimeout;
            targetRealm.Data.SsoSessionMaxLifespan = originRealm.Data.SsoSessionMaxLifespan;
            targetRealm.Data.OfflineSessionIdleTimeout = originRealm.Data.OfflineSessionIdleTimeout;
            targetRealm.Data.OfflineSessionMaxLifespan = originRealm.Data.OfflineSessionMaxLifespan;

            var updateResult = await realmRepository.UpdateRealmAsync(targetTenant, targetRealm.Data, cancellationToken);

            return updateResult.IsSuccess;
        }

        private async Task AssociatedRulesToTheClientAsync(string targetTenant, string originTenant, ClientEntity client, string clientId, CancellationToken cancellationToken)
        {
            var originClientRoles = await clientRoleRepository.GetRolesForClientAsync(client.Id, originTenant, cancellationToken);

            foreach (var clientRole in originClientRoles.Data)
            {
                await clientRoleRepository.AddClientRoleAsync(clientId,
                    clientRole.Name,
                    clientRole?.Description ?? "",
                    targetTenant,
                    cancellationToken);
            }
        }

        private async Task AssociateClientRulesToTheGroupAsync(string targetTenant, string adminGroupId, string clientId, CancellationToken cancellationToken)
        {
            var targetClientRulesAdded = await clientRoleRepository.GetRolesForClientAsync(clientId, targetTenant, cancellationToken);

            foreach (var targetClientRuleAdded in targetClientRulesAdded.Data)
            {
                await groupRolesRepository.AddClientRoleToGroupAsync(adminGroupId,
                    clientId,
                    targetClientRuleAdded.Id,
                    targetClientRuleAdded.Name,
                    targetTenant,
                    cancellationToken);
            }
        }

        private async Task AssociateUserToTheGroupAsync(string targetTenant, string adminGroupId, Domain.Entities.User user, CancellationToken cancellationToken)
        {
            await groupUsersRepository.AddUserToGroupAsync(user.Id, targetTenant, Guid.Parse(adminGroupId), cancellationToken);
        }

        private async Task<Domain.Entities.User> CreateUserAsync(ReplicateRealmCommand request, string targetTenant, CancellationToken cancellationToken)
        {
            var user = new Domain.Entities.User(request.ReplicateRealmRequest.ReplicationConfigurationRequest.AdminUser.Username,
                request.ReplicateRealmRequest.ReplicationConfigurationRequest.AdminUser.Password,
                request.ReplicateRealmRequest.ReplicationConfigurationRequest.AdminUser.Username,
                request.ReplicateRealmRequest.ReplicationConfigurationRequest.AdminUser.Username,
                request.ReplicateRealmRequest.ReplicationConfigurationRequest.AdminUser.Username,
                 new Dictionary<string, string[]>
                 {
                     { "Tenant", [targetTenant] }
                 });

            var creationUserResult = await userRepository.CreateAsync(user, cancellationToken);

            user.Id = Guid.Parse(creationUserResult.Data);

            await userRepository.ResetPasswordAsync(user.Id, user.Password, targetTenant, cancellationToken);

            return user;
        }

        private async Task<string> CreateAdminGroupWithAllRulesAssociatedAsync(ReplicateRealmCommand request, string targetTenant, CancellationToken cancellationToken)
        {
            if (request.ReplicateRealmRequest!.ReplicationConfigurationRequest.CreateAdminGroupWithAllRulesAssociated)
            {
                return (await groupRepository.CreateAsync(Constants.AdminGroupName, targetTenant, [], cancellationToken)).Data;
            }

            return string.Empty;
        }
    }
}
