using Feijuca.Auth.Models;
using Feijuca.Auth.Application.Mappers;
using Feijuca.Auth.Domain.Interfaces;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public class AddUserCommandHandler(IUserRepository userRepository) : ICommandHandler<AddUserCommand, Result<Guid>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Result<Guid>> HandleAsync(AddUserCommand request, CancellationToken cancellationToken = default)
        {
            var user = request.AddUserRequest.ToDomain(request.Tenant);
            var result = await _userRepository.CreateAsync(user, cancellationToken);

            if (result.IsSuccess)
            {
                var keycloakUser = await _userRepository.GetAsync(user.Username, request.Tenant, cancellationToken);
                var resultResetPassword = await _userRepository.ResetPasswordAsync(keycloakUser.Data.Id, user.Password, request.Tenant, cancellationToken);

                if (resultResetPassword.IsSuccess)
                {
                    return Result<Guid>.Success(keycloakUser.Data.Id);
                }
            }

            return Result<Guid>.Failure(result.Error);
        }
    }
}
