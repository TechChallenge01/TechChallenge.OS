using Domain.Aggregates.OrdemServicoAggregates;
using Domain.Enums;
using FluentAssertions;
using Reqnroll;

namespace BDD.Steps;

[Binding]
public class SagaOSSteps
{
    private OrdemServico? _os;
    private Exception? _excecao;
    private readonly Guid _idUsuario = Guid.NewGuid();
    private readonly Guid _clienteId = Guid.NewGuid();
    private readonly Guid _veiculoId = Guid.NewGuid();

    [Given(@"uma Ordem de Servico e criada para o cliente com descricao ""(.*)""")]
    public void GivenUmaOrdemDeServico(string descricao)
    {
        _os = new OrdemServico(_clienteId, _veiculoId, descricao, _idUsuario);
    }

    [When(@"o tecnico inicia o diagnostico")]
    public void WhenIniciarDiagnostico() => _os!.IniciarDiagnostico(_idUsuario);

    [When(@"o tecnico registra o diagnostico ""(.*)"" com valor estimado de (.*)")]
    public void WhenRegistrarDiagnostico(string diagnostico, decimal valor)
        => _os!.RegistrarDiagnostico(diagnostico, valor, _idUsuario);

    [When(@"o pagamento e aprovado pelo sistema")]
    public void WhenPagamentoAprovado() => _os!.IniciarExecucao(_idUsuario);

    [When(@"a execucao e concluida com valor final de (.*)")]
    public void WhenExecucaoConcluida(decimal valorFinal) => _os!.Finalizar(valorFinal, _idUsuario);

    [When(@"a entrega e registrada")]
    public void WhenEntregaRegistrada() => _os!.RegistrarEntrega(_idUsuario);

    [When(@"a OS e cancelada pelo tecnico com motivo ""(.*)""")]
    public void WhenCancelar(string motivo) => _os!.Cancelar(_idUsuario, motivo);

    [When(@"uma transicao invalida e tentada")]
    public void WhenTransicaoInvalida()
    {
        _excecao = Record.Exception(() => _os!.Finalizar(0m, _idUsuario));
    }

    [Then(@"a Ordem de Servico deve ter status ""(.*)""")]
    public void ThenStatus(string status)
        => _os!.Status.ToString().Should().Be(status);

    [Then(@"o historico deve conter (.*) entradas")]
    public void ThenHistoricoCount(int count)
        => _os!.Historico.Count.Should().Be(count);

    [Then(@"uma excecao de operacao invalida deve ser lancada")]
    public void ThenExcecao()
        => _excecao.Should().BeOfType<InvalidOperationException>();
}
