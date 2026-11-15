using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Shared.DTOs.OrdemServicos.Request;
using Xunit;

namespace UnitTests.Application;

public class RegistrarDiagnosticoUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private RegistrarDiagnosticoUseCase CriarUseCase()
        => RegistrarDiagnosticoUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object), _mockPublisher.Object);

    private static OrdemServicoInputDTO OSEmDiagnosticoDTO() => new()
    {
        Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Problema", Status = EStatusOS.EmDiagnostico.ToString(),
        Historico = new() { new() { Status = "EmDiagnostico", DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() } },
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoOSExiste_DeveRegistrarDiagnosticoEPublicar()
    {
        var dto = OSEmDiagnosticoDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegistrarDiagnosticoRequestDTO { Diagnostico = "Motor desgastado", ValorEstimado = 1500m };
        await CriarUseCase().Run(dto.Id, request, Guid.NewGuid(), CancellationToken.None);

        _mockDS.Verify(x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == "AguardandoAprovacao" && d.Diagnostico == "Motor desgastado"), It.IsAny<CancellationToken>()), Times.Once);
        _mockPublisher.Verify(x => x.PublishAsync("diagnostico.concluido", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoNaoEncontrada_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var request = new RegistrarDiagnosticoRequestDTO { Diagnostico = "X", ValorEstimado = 100m };
        var act = () => CriarUseCase().Run(Guid.NewGuid(), request, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
