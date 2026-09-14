using AutoFixture;
using Feijuca.Auth.Application.Commands.User;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using FluentAssertions;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Command.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IUserRepository> userRepository = new();
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _handler = new LoginCommandHandler(userRepository.Object);
        }

        [Fact(Skip = "FIX LATER")]
        public async Task Given_ValidLogin_When_LoginAsync_Then_ShouldReturnSuccess()
        {
            // Arrange
            var loginCommand = _fixture.Create<LoginCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var tokenDetails = _fixture.Create<TokenDetails>();
            var loginResult = Result<TokenDetails>.Success(tokenDetails);

            userRepository
                .Setup(repo => repo.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loginResult);

            // Act
            var result = await _handler.HandleAsync(loginCommand, cancellationToken);

            // Assert
            result
                .IsSuccess
                .Should()
                .BeTrue();

            userRepository.Verify(repo => repo.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
            userRepository.VerifyNoOtherCalls();
        }

        [Fact(Skip = "FIX LATER")]
        public async Task Given_InvalidLogin_When_LoginAsync_Then_ShouldReturnFailure()
        {
            // Arrange
            var loginCommand = _fixture.Create<LoginCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var loginResult = Result<TokenDetails>.Failure(UserErrors.InvalidUserNameOrPasswordError);

            userRepository
                .Setup(repo => repo.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(loginResult);

            // Act
            var result = await _handler.HandleAsync(loginCommand, cancellationToken);

            // Assert
            result
                .Error
                .Should()
                .Be(UserErrors.InvalidUserNameOrPasswordError);

            userRepository.Verify(repo => repo.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
            userRepository.VerifyNoOtherCalls();
        }
    }
}
