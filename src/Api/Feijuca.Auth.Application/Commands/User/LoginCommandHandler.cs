using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Http.Responses;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public class LoginCommandHandler(IUserRepository userRepository, ITenantProvider tenantService) : ICommandHandler<LoginCommand, Result<TokenDetailsResponse>>
    {
        public async Task<Result<TokenDetailsResponse>> HandleAsync(LoginCommand request, CancellationToken cancellationToken)
        {
            tenantService.SetTenants([new Tenant(request.Tenant)]);

            var result = await userRepository.LoginAsync(request.LoginUser.Username, request.LoginUser.Password, cancellationToken);

            return result.IsSuccess ? Result<TokenDetailsResponse>.Success(result.Data.ToTokenDetailResponse()) : Result<TokenDetailsResponse>.Failure(result.Error);
        }
    }
}