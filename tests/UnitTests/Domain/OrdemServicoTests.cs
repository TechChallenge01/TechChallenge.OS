using Domain.Aggregates.OrdemServicoAggregates;
using Domain.Enums;
using FluentAssertions;
using Xunit;

namespace UnitTests.Domain;

public class OrdemServicoTests
{
    private static readonly Guid ClienteId = Guid.NewGuid();
    private static readonly Guid VeiculoId = Guid.NewGuid();
    private static readonly Guid UsuarioId = Guid.NewGuid();

    private static OrdemServico CriarOS() =>
        new(ClienteId, VeiculoId, "Troca de óleo", UsuarioId);

    [Fact]
    public void CriarOS_DeveIniciarComStatusRecebida()
    {
        var os = CriarOS();
        os.Status.Should().Be(EStatusOS.Recebida);
        os.Ativo.Should().BeTrue();
        os.ClienteId.Should().Be(ClienteId);
        os.VeiculoId.Should().Be(VeiculoId);
        os.DescricaoProblema.Should().Be("Troca de óleo");
    }

    [Fact]
    public void IniciarDiagnostico_DeveTransicionarParaEmDiagnostico()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.Status.Should().Be(EStatusOS.EmDiagnostico);
    }

    [Fact]
    public void IniciarDiagnostico_DeveAdicionarHistorico()
    {
        var os = CriarOS();
        var historicoAntes = os.Historico.Count;
        os.IniciarDiagnostico(UsuarioId);
        os.Historico.Count.Should().Be(historicoAntes + 1);
        os.Historico.Last().Status.Should().Be(EStatusOS.EmDiagnostico.ToString());
    }

    [Fact]
    public void RegistrarDiagnostico_DeveTransicionarParaAguardandoAprovacao()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Motor desgastado", 1500m, UsuarioId);
        os.Status.Should().Be(EStatusOS.AguardandoAprovacao);
    }

    [Fact]
    public void RegistrarDiagnostico_DeveDefinirDiagnosticoEValorEstimado()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Motor desgastado", 1500m, UsuarioId);
        os.Diagnostico.Should().Be("Motor desgastado");
        os.ValorEstimado.Should().Be(1500m);
    }

    [Fact]
    public void IniciarExecucao_DeveTransicionarParaEmExecucao()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Diagnóstico", 1000m, UsuarioId);
        os.IniciarExecucao(UsuarioId);
        os.Status.Should().Be(EStatusOS.EmExecucao);
    }

    [Fact]
    public void Finalizar_DeveDefinirValorFinalEStatus()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Diagnóstico", 1000m, UsuarioId);
        os.IniciarExecucao(UsuarioId);
        os.Finalizar(950m, UsuarioId);
        os.Status.Should().Be(EStatusOS.Finalizada);
        os.ValorFinal.Should().Be(950m);
    }

    [Fact]
    public void RegistrarEntrega_DeveTransicionarParaEntregue()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Diagnóstico", 1000m, UsuarioId);
        os.IniciarExecucao(UsuarioId);
        os.Finalizar(1000m, UsuarioId);
        os.RegistrarEntrega(UsuarioId);
        os.Status.Should().Be(EStatusOS.Entregue);
    }

    [Fact]
    public void Cancelar_QuandoRecebida_DeveCancelar()
    {
        var os = CriarOS();
        os.Cancelar(UsuarioId, "Cliente desistiu");
        os.Status.Should().Be(EStatusOS.Cancelada);
    }

    [Fact]
    public void Cancelar_QuandoFinalizada_DeveLancarException()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Diagnóstico", 1000m, UsuarioId);
        os.IniciarExecucao(UsuarioId);
        os.Finalizar(1000m, UsuarioId);

        var act = () => os.Cancelar(UsuarioId, "motivo");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancelar_QuandoEntregue_DeveLancarException()
    {
        var os = CriarOS();
        os.IniciarDiagnostico(UsuarioId);
        os.RegistrarDiagnostico("Diagnóstico", 1000m, UsuarioId);
        os.IniciarExecucao(UsuarioId);
        os.Finalizar(1000m, UsuarioId);
        os.RegistrarEntrega(UsuarioId);

        var act = () => os.Cancelar(UsuarioId, "motivo");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidarTransicao_TransicaoInvalida_DeveLancarException()
    {
        var os = CriarOS();
        // Recebida → Finalizada (inválido)
        var act = () => os.Finalizar(1000m, UsuarioId);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Transição de status inválida*");
    }

    [Fact]
    public void ValidarTransicao_RecebidaParaEmDiagnostico_NaoDeveLancar()
    {
        var os = CriarOS();
        var act = () => os.IniciarDiagnostico(UsuarioId);
        act.Should().NotThrow();
    }

    [Fact]
    public void CriarOS_DeveAdicionarHistoricoInicial()
    {
        var os = CriarOS();
        os.Historico.Should().HaveCount(1);
        os.Historico[0].Status.Should().Be(EStatusOS.Recebida.ToString());
    }

    [Fact]
    public void Inativar_DeveDesativarOS()
    {
        var os = CriarOS();
        os.Inativar();
        os.Ativo.Should().BeFalse();
    }
}
