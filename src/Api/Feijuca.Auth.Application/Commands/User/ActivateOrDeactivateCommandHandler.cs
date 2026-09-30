using Feijuca.Auth.Models;
using Feijuca.Auth.Domain.Interfaces;
using LiteBus.Commands.Abstractions;

namespace Feijuca.Auth.Application.Commands.User
{
    public class ActivateOrDeactivateCommandHandler(IUserRepository userRepository) : ICommandHandler<ActivateOrDeactivateCommand, Result<bool>>
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<Result<bool>> HandleAsync(ActivateOrDeactivateCommand request, CancellationToken cancellationToken)
        {
            var result = await _userRepository.ActivateOrDeactivateAsync(request.Id, request.Request.IsActive, cancellationToken);

            if (result.IsSuccess)
            {
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure(result.Error);
        }
    }
}