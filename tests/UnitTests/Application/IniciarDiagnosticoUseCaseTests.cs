using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Xunit;

namespace UnitTests.Application;

public class IniciarDiagnosticoUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _mockDS = new();

    private IniciarDiagnosticoUseCase CriarUseCase()
        => IniciarDiagnosticoUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object));

    private static OrdemServicoInputDTO OSRecebidaDTO() => new()
    {
        Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Problema", Status = EStatusOS.Recebida.ToString(),
        Historico = new() { new() { Status = "Recebida", DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() } },
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoOSExiste_DeveIniciarDiagnostico()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await CriarUseCase().Run(dto.Id, Guid.NewGuid(), CancellationToken.None);

        _mockDS.Verify(x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == "EmDiagnostico"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Run_QuandoNaoEncontrada_DeveLancarException()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var act = () => CriarUseCase().Run(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
