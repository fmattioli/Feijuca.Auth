using AutoFixture;
using Feijuca.Auth.Application.Queries.Clients;
using Feijuca.Auth.Common.Errors;
using Feijuca.Auth.Domain.Entities;
using Feijuca.Auth.Domain.Interfaces;
using Feijuca.Auth.Models;
using Feijuca.Auth.Providers;
using FluentAssertions;
using Moq;

namespace Feijuca.Auth.Api.UnitTests.Queries.Clients
{
    public class GetAllClientsQueryHandlerTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<IClientRepository> _clientRepositoryMock = new();
        private readonly Mock<ITenantProvider> _tenantProviderMock = new();
        private readonly GetAllClientsQueryHandler _handler;

        public GetAllClientsQueryHandlerTests()
        {
            _handler = new GetAllClientsQueryHandler(_clientRepositoryMock.Object, _tenantProviderMock.Object);
        }

        [Fact]
        public async Task Given_ClientsExist_When_HandleIsCalled_Then_ShouldReturnSuccess()
        {
            // Arange
            var clientsQuery = _fixture.Create<GetAllClientsQuery>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var clients = _fixture.CreateMany<ClientEntity>();
            var clientsResult = Result<IEnumerable<ClientEntity>>.Success(clients);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _clientRepositoryMock
                .Setup(repo => repo.GetClientsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientsResult);

            // Act
            var result = await _handler.HandleAsync(clientsQuery, cancellationToken);

            // Assert
            result
                .Should()
                .NotBeEmpty();

            _clientRepositoryMock.Verify(repo => repo.GetClientsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
            _clientRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Given_ClientsInvalid_When_HandleIsCalled_Then_ShouldReturnFailure()
        {
            // Arange
            var clientsQuery = _fixture.Create<GetAllClientsQuery>();
            var cancellationToken = _fixture.Create<CancellationToken>();
            var clientsResult = Result<IEnumerable<ClientEntity>>.Failure(ClientErrors.GetClientsErrors);

            _tenantProviderMock
                .Setup(provider => provider.Tenant)
                .Returns(_fixture.Create<Tenant>());

            _clientRepositoryMock
                .Setup(repo => repo.GetClientsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientsResult);

            // Act
            var result = await _handler.HandleAsync(clientsQuery, cancellationToken);

            // Assert
            result
                .Should()
                .BeEmpty();

            _clientRepositoryMock.Verify(repo => repo.GetClientsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once());
            _clientRepositoryMock.VerifyNoOtherCalls();
        }
    }
}