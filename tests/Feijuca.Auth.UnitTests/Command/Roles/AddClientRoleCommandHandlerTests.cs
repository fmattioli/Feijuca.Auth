using AutoFixture;
using Feijuca.Auth.Application.Commands.ClientRole;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Command.Roles
{
    public class AddClientRoleCommandHandlerTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IClientRoleRepository> _roleRepositoryMock = new();
        private readonly Mock<IRealmRepository> _realmRepositoryMock = new();
        private readonly Mock<ITenantProvider> _tenantProviderMock = new();
        private readonly Mock<ILogger<AddClientRoleCommandHandler>> _loggerMock = new();
        private readonly AddClientRoleCommandHandler _handler;

        public AddClientRoleCommandHandlerTests()
        {
            _handler = new AddClientRoleCommandHandler(_roleRepositoryMock.Object,
                _realmRepositoryMock.Object,
                _tenantProviderMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Given_ValidRoleData_When_AddRoleIsCalled_Then_ShouldAddRoleSuccessfully()
        {
            var addRoleCommand = _fixture.Create<AddClientRoleCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var addRoleResult = Result<bool>.Success(true);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _roleRepositoryMock
                .Setup(repo => repo.AddClientRoleAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(addRoleResult);

            // Act
            var result = await _handler.HandleAsync(addRoleCommand, cancellationToken);

            // Assert
            result
                .IsSuccess
                .Should()
                .BeTrue();

            _roleRepositoryMock.Verify(repo => repo.AddClientRoleAsync(It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()));

            _roleRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_InvalidRoleData_When_AddRoleIsCalled_Then_ShouldAddRoleFailure()
        {
            var addRoleCommand = _fixture.Create<AddClientRoleCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var addRoleResult = Result<bool>.Failure(RoleErrors.AddRoleErrors);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _roleRepositoryMock
                .Setup(repo => repo.AddClientRoleAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(addRoleResult);

            // Act
            var result = await _handler.HandleAsync(addRoleCommand, cancellationToken);

            // Assert
            result
                .Error
                .Should()
                .Be(RoleErrors.AddRoleErrors);

            _roleRepositoryMock.Verify(repo => repo.AddClientRoleAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(), 
                It.IsAny<CancellationToken>()));

            _roleRepositoryMock.VerifyNoOtherCalls();
        }
    }
}