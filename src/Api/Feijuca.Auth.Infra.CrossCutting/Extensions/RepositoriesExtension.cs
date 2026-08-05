using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Infra.Data.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Feijuca.Auth.Infra.CrossCutting.Extensions
{
    public static class RepositoriesExtension
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IConfigRepository, ConfigRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserRolesRepository, UserRolesRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IGroupUsersRepository, UserGroupRepository>();
            services.AddScoped<IClientRoleRepository, ClientRoleRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IRealmRepository, RealmRepository>();
            services.AddScoped<IGroupRolesRepository, GroupRolesRepository>();
            services.AddScoped<IClientScopesRepository, ClientScopesRepository>();

            return services;
        }
    }
}
