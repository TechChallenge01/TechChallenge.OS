using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Domain.Enums;
using FluentAssertions;
using Moq;
using Shared.DTOs.OrdemServicos.Input;
using Shared.DTOs.OrdemServicos.Request;
using System.Net;
using Xunit;

namespace UnitTests.Application;

public class OrdemServicoControllerTests
{
    private readonly Mock<IOrdemServicoDataSource> _mockDS = new();
    private readonly Mock<IEventPublisher> _mockPublisher = new();

    private OrdemServicoController CriarController() => new(_mockDS.Object);

    private static OrdemServicoInputDTO OSRecebidaDTO() => new()
    {
        Id = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Problema",
        Status = EStatusOS.Recebida.ToString(),
        Historico = [new() { Status = "Recebida", DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() }],
        IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow,
        Ativo = true
    };

    private static OrdemServicoInputDTO OSComStatus(EStatusOS status) => new()
    {
        Id = Guid.NewGuid(),
        ClienteId = Guid.NewGuid(),
        VeiculoId = Guid.NewGuid(),
        DescricaoProblema = "Problema",
        Status = status.ToString(),
        Historico = [new() { Status = status.ToString(), DataAlteracao = DateTime.UtcNow, IdUsuario = Guid.NewGuid() }],
        IdUsuarioCriacao = Guid.NewGuid(),
        DataCriacao = DateTime.UtcNow,
        Ativo = true
    };

    [Fact]
    public async Task Abrir_DeveRetornarCreated()
    {
        _mockDS.Setup(x => x.Create(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new AbrirOrdemServicoRequestDTO
        {
            ClienteId = Guid.NewGuid(),
            VeiculoId = Guid.NewGuid(),
            DescricaoProblema = "Motor"
        };

        var result = await CriarController().Abrir(request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetById_QuandoEncontrado_DeveRetornarOK()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

        var result = await CriarController().GetById(dto.Id, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var result = await CriarController().GetById(Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPaginated_DeveRetornarPartialContent()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetPaginated(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<OrdemServicoInputDTO> { dto }, 1));

        var result = await CriarController().GetPaginated(1, 10, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.PartialContent);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().IniciarDiagnostico(dto.Id, Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task IniciarDiagnostico_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var result = await CriarController().IniciarDiagnostico(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RegistrarDiagnostico_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = OSComStatus(EStatusOS.EmDiagnostico);
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegistrarDiagnosticoRequestDTO { Diagnostico = "Motor gasto", ValorEstimado = 1000m };
        var result = await CriarController().RegistrarDiagnostico(dto.Id, request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RegistrarDiagnostico_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var request = new RegistrarDiagnosticoRequestDTO { Diagnostico = "X", ValorEstimado = 100m };
        var result = await CriarController().RegistrarDiagnostico(Guid.NewGuid(), request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Cancelar_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = OSRecebidaDTO();
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new CancelarOrdemServicoRequestDTO { Motivo = "Cliente desistiu" };
        var result = await CriarController().Cancelar(dto.Id, request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Cancelar_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var request = new CancelarOrdemServicoRequestDTO { Motivo = "motivo" };
        var result = await CriarController().Cancelar(Guid.NewGuid(), request, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AtualizarStatus_TransicaoValida_DeveRetornarNoContent()
    {
        var dto = OSComStatus(EStatusOS.AguardandoAprovacao);
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().AtualizarStatus(dto.Id, EStatusOS.EmExecucao, Guid.NewGuid(), null, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task AtualizarStatus_OSNaoEncontrada_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var result = await CriarController().AtualizarStatus(Guid.NewGuid(), EStatusOS.Cancelada, Guid.NewGuid(), null, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Finalizar_QuandoEncontrado_DeveRetornarNoContent()
    {
        var dto = OSComStatus(EStatusOS.EmExecucao);
        _mockDS.Setup(x => x.GetById(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);
        _mockDS.Setup(x => x.Update(It.IsAny<OrdemServicoInputDTO>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockPublisher.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await CriarController().Finalizar(dto.Id, 1200m, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Finalizar_QuandoNaoEncontrado_DeveRetornarNotFound()
    {
        _mockDS.Setup(x => x.GetById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrdemServicoInputDTO?)null);

        var result = await CriarController().Finalizar(Guid.NewGuid(), 1200m, Guid.NewGuid(), _mockPublisher.Object, CancellationToken.None);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
