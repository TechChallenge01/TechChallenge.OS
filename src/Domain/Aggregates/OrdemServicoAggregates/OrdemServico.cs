using Domain.BaseEntity;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Aggregates.OrdemServicoAggregates;

public class OrdemServico : Base
{
    private readonly List<HistoricoStatus> _historico = new();

    public OrdemServico(Guid clienteId, Guid veiculoId, string descricaoProblema, Guid idUsuarioCriacao)
        : base(idUsuarioCriacao, DateTime.UtcNow)
    {
        Id = Guid.NewGuid();
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        DescricaoProblema = descricaoProblema;
        Status = EStatusOS.Recebida;

        _historico.Add(new HistoricoStatus(Status.ToString(), DateTime.UtcNow, idUsuarioCriacao, "OS criada"));
    }

    // Construtor de reconstituição (rehidratação)
    public OrdemServico(
        Guid id, Guid clienteId, Guid veiculoId, string descricaoProblema,
        EStatusOS status, decimal? valorEstimado, decimal? valorFinal,
        string? diagnostico, List<HistoricoStatus> historico,
        Guid idUsuarioCriacao, DateTime dataCriacao,
        Guid? idUsuarioAtualizacao, DateTime? dataAtualizacao, bool ativo)
        : base(idUsuarioCriacao, dataCriacao, idUsuarioAtualizacao, dataAtualizacao)
    {
        Id = id;
        ClienteId = clienteId;
        VeiculoId = veiculoId;
        DescricaoProblema = descricaoProblema;
        Status = status;
        ValorEstimado = valorEstimado;
        ValorFinal = valorFinal;
        Diagnostico = diagnostico;
        _historico = historico;
        Ativo = ativo;
    }

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid VeiculoId { get; private set; }
    public string DescricaoProblema { get; private set; } = string.Empty;
    public EStatusOS Status { get; private set; }
    public decimal? ValorEstimado { get; private set; }
    public decimal? ValorFinal { get; private set; }
    public string? Diagnostico { get; private set; }
    public IReadOnlyList<HistoricoStatus> Historico => _historico.AsReadOnly();

    public void IniciarDiagnostico(Guid idUsuario)
    {
        ValidarTransicao(EStatusOS.EmDiagnostico);
        Status = EStatusOS.EmDiagnostico;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.EmDiagnostico.ToString(), DateTime.UtcNow, idUsuario));
    }

    public void RegistrarDiagnostico(string diagnostico, decimal valorEstimado, Guid idUsuario)
    {
        ValidarTransicao(EStatusOS.AguardandoAprovacao);
        Diagnostico = diagnostico;
        ValorEstimado = valorEstimado;
        Status = EStatusOS.AguardandoAprovacao;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.AguardandoAprovacao.ToString(), DateTime.UtcNow, idUsuario,
            $"Diagnóstico registrado. Valor estimado: {valorEstimado:C}"));
    }

    public void IniciarExecucao(Guid idUsuario)
    {
        ValidarTransicao(EStatusOS.EmExecucao);
        Status = EStatusOS.EmExecucao;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.EmExecucao.ToString(), DateTime.UtcNow, idUsuario, "Pagamento confirmado — execução iniciada"));
    }

    public void Finalizar(decimal valorFinal, Guid idUsuario)
    {
        ValidarTransicao(EStatusOS.Finalizada);
        ValorFinal = valorFinal;
        Status = EStatusOS.Finalizada;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.Finalizada.ToString(), DateTime.UtcNow, idUsuario,
            $"Reparo concluído. Valor final: {valorFinal:C}"));
    }

    public void RegistrarEntrega(Guid idUsuario)
    {
        ValidarTransicao(EStatusOS.Entregue);
        Status = EStatusOS.Entregue;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.Entregue.ToString(), DateTime.UtcNow, idUsuario));
    }

    public void Cancelar(Guid idUsuario, string motivo)
    {
        if (Status is EStatusOS.Finalizada or EStatusOS.Entregue)
            throw new InvalidOperationException($"Não é possível cancelar uma OS com status {Status}.");

        Status = EStatusOS.Cancelada;
        RastrearAlteracao(idUsuario, DateTime.UtcNow);
        _historico.Add(new HistoricoStatus(EStatusOS.Cancelada.ToString(), DateTime.UtcNow, idUsuario, motivo));
    }

    private void ValidarTransicao(EStatusOS novoStatus)
    {
        var transicoesValidas = new Dictionary<EStatusOS, List<EStatusOS>>
        {
            { EStatusOS.Recebida,            new() { EStatusOS.EmDiagnostico, EStatusOS.Cancelada } },
            { EStatusOS.EmDiagnostico,       new() { EStatusOS.AguardandoAprovacao, EStatusOS.Cancelada } },
            { EStatusOS.AguardandoAprovacao, new() { EStatusOS.EmExecucao, EStatusOS.Cancelada } },
            { EStatusOS.EmExecucao,          new() { EStatusOS.Finalizada, EStatusOS.Cancelada } },
            { EStatusOS.Finalizada,          new() { EStatusOS.Entregue } },
            { EStatusOS.Entregue,            new() { } },
            { EStatusOS.Cancelada,           new() { } }
        };

        if (!transicoesValidas.TryGetValue(Status, out var permitidos) || !permitidos.Contains(novoStatus))
            throw new InvalidOperationException($"Transição de status inválida: {Status} → {novoStatus}.");
    }
}
