using AutoFixture;
using Feijuca.Auth.Application.Commands.GroupUser;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using FluentAssertions;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Command.GroupUser
{
    public class AddUserToGroupCommandHandlerTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IGroupUsersRepository> _groupUsersRepositoryMock = new();
        private readonly Mock<ITenantProvider> _tenantProviderMock = new();
        private readonly AddUserToGroupCommandHandler _handler;

        public AddUserToGroupCommandHandlerTests()
        {
            _handler = new AddUserToGroupCommandHandler(_groupUsersRepositoryMock.Object, _tenantProviderMock.Object);
        }

        [Fact]
        public async Task Given_ValidUserAndGroup_When_AddUserToGroupIsCalled_Then_ShouldAddUserSuccessfully()
        {
            // Arrange
            var groupUserCommand = _fixture.Create<AddUserToGroupCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var groupUserResult = Result<bool>.Success(true);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _groupUsersRepositoryMock
                .Setup(repo => repo.AddUserToGroupAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupUserResult);

            // Act
            var result = await _handler.HandleAsync(groupUserCommand, cancellationToken);

            // Assert
            result
                .IsSuccess
                .Should()
                .BeTrue();

            _groupUsersRepositoryMock.Verify(repo => repo.AddUserToGroupAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once());
            _groupUsersRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_InvalidUserAndGroup_When_AddUserToGroupIsCalled_Then_ShouldReturnFails()
        {
            // Arrange
            var groupUserCommand = _fixture.Create<AddUserToGroupCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var groupUserResult = Result<bool>.Failure(UserGroupErrors.ErrorAddUserToGroup);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _groupUsersRepositoryMock
                .Setup(repo => repo.AddUserToGroupAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupUserResult);

            // Act
            var result = await _handler.HandleAsync(groupUserCommand, cancellationToken);

            // Assert
            result
                .Error
                .Should()
                .Be(UserGroupErrors.ErrorAddUserToGroup);

            _groupUsersRepositoryMock.Verify(repo => repo.AddUserToGroupAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once());
            _groupUsersRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
