using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Xunit;

namespace UnitTests.Application;

public class FinalizarOrdemServicoUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private FinalizarOrdemServicoUseCase CriarUseCase()
        => FinalizarOrdemServicoUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object), _mockPublisher.Object);

    private static OrdemServicoInputDTO OSEmExecucaoDTO() => new()
    {
        Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Troca de motor", Status = EStatusOS.EmExecucao.ToString(),
        Historico = new() { new() { Status = "EmExecucao", DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() } },
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoOSExiste_DeveFinalizarEPublicar()
    {
        var dto = OSEmExecucaoDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(dto.Id, 1200m, Guid.NewGuid(), CancellationToken.None);

        _mockDS.Verify(x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == "Finalizada" && d.ValorFinal == 1200m), It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.PublishAsync("execucao.finalizada", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoNaoEncontrada_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var act = () => CriarUseCase().Run(Guid.NewGuid(), 100m, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
