using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Xunit;

namespace UnitTests.Application;

public class GetByIdUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _mockDS = new();

    private static OrdemServicoInputDTO OSRecebidaDTO() => new()
    {
        Id = Guid.NewGuid(), ClienteId = Guid.NewGuid(), VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Problema", Status = EStatusOS.Recebida.ToString(),
        Historico = new() { new() { Status = "Recebida", DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() } },
        IdUsuarioCriacao = Guid.NewGuid(), DataCriacao = DateTime.UtcNow, Ativo = true
    };

    [Fact]
    public async Task Run_QuandoExiste_DeveRetornarOS()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var useCase = GetByIdUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object));
        var result = await useCase.Run(dto.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(dto.Id);
    }

    [Fact]
    public async Task Run_QuandoNaoExiste_DeveRetornarNull()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var useCase = GetByIdUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object));
        var result = await useCase.Run(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }
}
