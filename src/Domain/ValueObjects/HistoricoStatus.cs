namespace Domain.ValueObjects;

public class HistoricoStatus
{
    public HistoricoStatus(string status, DateTime dataAlteracao, Guid idUsuario, string? observacao = null)
    {
        Status = status;
        DataAlteracao = dataAlteracao;
        IdUsuario = idUsuario;
        Observacao = observacao;
    }

    public string Status { get; }
    public DateTime DataAlteracao { get; }
    public Guid IdUsuario { get; }
    public string? Observacao { get; }
}
