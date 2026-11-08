using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Shared.DTOs.OrdemServicos.Request;
using Xunit;

namespace UnitTests.Application;

public class AbrirOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _dataSourceMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly AbrirOrdemServicoUseCase _useCase;

    public AbrirOrdemServicoUseCaseTests()
    {
        _dataSourceMock = new Mock<IOrdemServicoDataSource>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _dataSourceMock
            .Setup(x => x.Create(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _eventPublisherMock
            .Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var gateway = OrdemServicoGateway.Create(_dataSourceMock.Object);
        _useCase = AbrirOrdemServicoUseCase.Create(gateway, _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Run_DeveRetornarIdDaOS()
    {
        var request = new AbrirOrdemServicoRequestDTO
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Verificação geral"
        };

        var id = await _useCase.Run(request, Guid.NewGuid(), CancellationToken.None);

        id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Run_DeveChamarGatewayCreate()
    {
        var request = new AbrirOrdemServicoRequestDTO
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Troca de óleo"
        };

        await _useCase.Run(request, Guid.NewGuid(), CancellationToken.None);

        _dataSourceMock.Verify(
            x => x.Create(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_DevePublicarEventoOsAberta()
    {
        var request = new AbrirOrdemServicoRequestDTO
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Revisão"
        };

        await _useCase.Run(request, Guid.NewGuid(), CancellationToken.None);

        _eventPublisherMock.Verify(
            x => x.PublishAsync("os.aberta", It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
