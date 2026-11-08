namespace Shared.DTOs.OrdemServicos.Input;

public class OrdemServicoInputDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? ValorEstimado { get; set; }
    public decimal? ValorFinal { get; set; }
    public string? Diagnostico { get; set; }
    public List<HistoricoStatusInputDTO> Historico { get; set; } = new();
    public Guid IdUsuarioCriacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public Guid? IdUsuarioAtualizacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
}

public class HistoricoStatusInputDTO
{
    public string Status { get; set; } = string.Empty;
    public DateTime DataAlteracao { get; set; }
    public Guid IdUsuario { get; set; }
    public string? Observacao { get; set; }
}
