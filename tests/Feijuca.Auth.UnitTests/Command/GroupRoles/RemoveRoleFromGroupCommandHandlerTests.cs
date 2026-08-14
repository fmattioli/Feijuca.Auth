using AutoFixture;
using Feijuca.Auth.Application.Commands.GroupRoles;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using FluentAssertions;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Command.GroupRoles
{
    public class RemoveRoleFromGroupCommandHandlerTests
    {
        private readonly Fixture _fixture = new();
        private readonly Mock<IGroupRepository> _groupRepositoryMock = new();
        private readonly Mock<IGroupRolesRepository> _groupRolesRepositoryMock = new();
        private readonly Mock<IClientRoleRepository> _roleRepositoryMock = new();
        private readonly Mock<ITenantProvider> _tenantProviderMock = new();
        private readonly RemoveRoleFromGroupCommandHandler _handler;

        public RemoveRoleFromGroupCommandHandlerTests()
        {
            _handler = new RemoveRoleFromGroupCommandHandler(
                _groupRepositoryMock.Object, 
                _groupRolesRepositoryMock.Object, 
                _roleRepositoryMock.Object,
                _tenantProviderMock.Object);
        }

        [Fact]
        public async Task Given_ValidGroupAndRole_When_RemoveRoleFromGroupIsCalled_Then_ShouldRemoveRoleSuccessfully()
        {
            // Arrange
            var command = _fixture.Create<RemoveRoleFromGroupCommand>();
            var cancellationToken = _fixture.Create<CancellationToken>();

            var groups = _fixture.Build<Group>()
                     .With(g => g.Id, command.GroupId)
                     .CreateMany(1);

            var roles = _fixture.Build<Role>()
                    .With(r => r.Id, command.RemoveRoleFromGroupRequest.RoleId)
                    .With(r => r.Name, "ValidRoleName")
                    .CreateMany(1);

            var groupsResult = Result<IEnumerable<Group>>.Success(groups);
            var rolesResult = Result<IEnumerable<Role>>.Success(roles);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _groupRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupsResult);

            _roleRepositoryMock
                .Setup(repo => repo.GetRolesForClientAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rolesResult);

            _groupRolesRepositoryMock
                .Setup(repo => repo.RemoveRoleFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupsResult);

            // Act
            var result = await _handler.HandleAsync(command, cancellationToken);

            // Assert
            result
                .IsSuccess
                .Should()
                .BeTrue();

            _groupRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _roleRepositoryMock.Verify(repo => repo.GetRolesForClientAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

            _groupRolesRepositoryMock
                .Verify(repo => repo.RemoveRoleFromGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()));

            _groupRepositoryMock.VerifyNoOtherCalls();
            _roleRepositoryMock.VerifyNoOtherCalls();
            _groupRolesRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_RepositoryFails_When_RemoveRoleFromGroupIsCalled_Then_ShouldReturnFailure()
        {
            // Arrange
            var fixture = new Fixture();
            var command = fixture.Create<RemoveRoleFromGroupCommand>();
            var cancellationToken = fixture.Create<CancellationToken>();

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            var groupsResult = Result<IEnumerable<Group>>.Failure(GroupRolesErrors.RemovingRoleFromGroupError);

            _groupRepositoryMock
                .Setup(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(groupsResult);

            // Act
            var result = await _handler.HandleAsync(command, cancellationToken);

            // Assert
            result
                .Error
                .Should()
                .Be(GroupRolesErrors.RemovingRoleFromGroupError);

            _groupRepositoryMock.Verify(repo => repo.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _groupRolesRepositoryMock.VerifyNoOtherCalls();
        }
    }
}
