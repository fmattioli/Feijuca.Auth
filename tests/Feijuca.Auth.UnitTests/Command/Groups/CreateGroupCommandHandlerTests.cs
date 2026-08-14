using AutoFixture;
using Feijuca.Auth.Application.Commands.Group;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using FluentAssertions;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Command.Groups
{
    public class CreateGroupCommandHandlerTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IGroupRepository> _groupRepositoryMock = new();
        private readonly Mock<ITenantProvider> _tenantProviderMock = new();
        private readonly AddGroupCommandHandler _handler;

        public CreateGroupCommandHandlerTests()
        {
            _handler = new AddGroupCommandHandler(_groupRepositoryMock.Object, _tenantProviderMock.Object);
        }

        [Fact]
        public async Task Given_ValidRequest_When_CreateGroupIsCalled_Then_ShouldReturnSuccess()
        {
            // Arrange
            var createGroupCommand = _fixture.Create<AddGroupCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _groupRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<string>.Success(_fixture.Create<string>()));

            // Act
            var result = await _handler.HandleAsync(createGroupCommand, cancellationToken);

            // Assert
            result
                .IsSuccess
                .Should()
                .BeTrue();

            _groupRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string[]>>(), It.IsAny<CancellationToken>()), Times.Once);
            _groupRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_InvalidRequest_When_CreateGroupFails_Then_ShouldReturnFailure()
        {
            // Arrange
            var createGroupCommand = _fixture.Create<AddGroupCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();

            var failureResult = Result<string>.Failure(GroupErrors.CreationGroupError);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _groupRepositoryMock
                .Setup(repo => repo.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string[]>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failureResult);

            // Act
            var result = await _handler.HandleAsync(createGroupCommand, cancellationToken);

            // Assert
            result
                .Error
                .Should()
                .Be(GroupErrors.CreationGroupError);

            _groupRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string[]>>(), It.IsAny<CancellationToken>()), Times.Once);
            _groupRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
