using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Interfaces;
using LiteBus.Commands.Abstractions;
using Feijuca.Auth.Common.Errors;

namespace Feijuca.Auth.Application.Commands.User
{
    public class ActivateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<ActivateUserCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Result<bool>> HandleAsync(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.ActivateUserAsync(request.Id, request.Request.IsActive, cancellationToken);

            if (result.IsSuccess)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(UserErrors.UpdateUserStatusError);
        }
    }
}