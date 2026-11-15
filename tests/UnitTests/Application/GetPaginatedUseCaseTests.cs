using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Xunit;

namespace UnitTests.Application;

public class GetPaginatedUseCaseTests
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
    public async Task Run_DeveRetornarPaginado()
    {
        var dtos = new List<OrdemServicoInputDTO> { OSRecebidaDTO(), OSRecebidaDTO() };
        _mockDS.Setup(x => x.GetPaginated(1, 10, It.IsAny<CancellationToken>())).ReturnsAsync((dtos, 2));

        var useCase = GetPaginatedUseCase.Create(OrdemServicoGateway.Create(_mockDS.Object));
        var (items, total) = await useCase.Run(1, 10, CancellationToken.None);

        items.Should().HaveCount(2);
        total.Should().Be(2);
    }
}
