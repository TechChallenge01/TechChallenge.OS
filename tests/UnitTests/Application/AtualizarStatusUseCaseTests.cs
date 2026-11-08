using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Xunit;

namespace UnitTests.Application;

public class AtualizarStatusUseCaseTests
{
    private readonly Mock<IOrdemServicoDataSource> _dataSourceMock;
    private readonly AtualizarStatusUseCase _useCase;
    private readonly Guid _osId = Guid.NewGuid();
    private readonly Guid _usuarioId = Guid.NewGuid();

    public AtualizarStatusUseCaseTests()
    {
        _dataSourceMock = new Mock<IOrdemServicoDataSource>();

        _dataSourceMock
            .Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var gateway = OrdemServicoGateway.Create(_dataSourceMock.Object);
        _useCase = AtualizarStatusUseCase.Create(gateway);
    }

    private void SetupOSComStatus(EStatusOS status)
    {
        var dto = new OrdemServicoInputDTO
        {
            Id = _osId,
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Test",
            Status = status.ToString(),
            Historico = new List<HistoricoStatusInputDTO>
            {
                new() { Status = status.ToString(), DataAlteracao = DateTime.UtcNow, IdUsuario = _usuarioId }
            },
            IdUsuarioCriacao = _usuarioId,
            DataCriacao = DateTime.UtcNow,
            Ativo = true
        };

        _dataSourceMock
            .Setup(x => x.GetById(_osId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);
    }

    [Fact]
    public async Task Run_OSNaoEncontrada_DeveLancarKeyNotFoundException()
    {
        _dataSourceMock
            .Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var act = async () => await _useCase.Run(Guid.NewGuid(), EStatusOS.Cancelada, _usuarioId, null, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Run_TransicionarParaEmExecucao_DeveAtualizarStatus()
    {
        SetupOSComStatus(EStatusOS.AguardandoAprovacao);

        await _useCase.Run(_osId, EStatusOS.EmExecucao, _usuarioId, null, CancellationToken.None);

        _dataSourceMock.Verify(
            x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == EStatusOS.EmExecucao.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_TransicionarParaFinalizada_DeveLancarInvalidOperationException()
    {
        SetupOSComStatus(EStatusOS.EmExecucao);

        var act = async () => await _useCase.Run(_osId, EStatusOS.Finalizada, _usuarioId, null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*endpoint específico*");
    }

    [Fact]
    public async Task Run_TransicionarParaEntregue_DeveAtualizarStatus()
    {
        SetupOSComStatus(EStatusOS.Finalizada);

        await _useCase.Run(_osId, EStatusOS.Entregue, _usuarioId, null, CancellationToken.None);

        _dataSourceMock.Verify(
            x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == EStatusOS.Entregue.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_TransicionarParaCancelada_DeveAtualizarStatus()
    {
        SetupOSComStatus(EStatusOS.Recebida);

        await _useCase.Run(_osId, EStatusOS.Cancelada, _usuarioId, "Cancelado pelo cliente", CancellationToken.None);

        _dataSourceMock.Verify(
            x => x.Update(It.Is<OrdemServicoInputDTO>(d => d.Status == EStatusOS.Cancelada.ToString()), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_StatusPadrao_DeveLancarInvalidOperationException()
    {
        SetupOSComStatus(EStatusOS.Recebida);

        var act = async () => await _useCase.Run(_osId, EStatusOS.AguardandoAprovacao, _usuarioId, null, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
