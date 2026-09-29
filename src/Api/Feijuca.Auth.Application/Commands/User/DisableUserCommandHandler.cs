using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Interfaces;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public class DisableUserCommandHandler(IUserRepository UserRepository) : ICommandHandler<DisableUserCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository = UserRepository;

        public async Task<Result<bool>> HandleAsync(DisableUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.DisableAsync(request.Id, cancellationToken);

            if (result.IsSuccess)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserErrors.DisableUserError);
        }
    }
}
