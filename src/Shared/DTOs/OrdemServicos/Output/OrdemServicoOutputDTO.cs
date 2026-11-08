namespace Shared.DTOs.OrdemServicos.Output;

public class OrdemServicoOutputDTO
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal? ValorEstimado { get; set; }
    public decimal? ValorFinal { get; set; }
    public string? Diagnostico { get; set; }
    public List<HistoricoStatusOutputDTO> Historico { get; set; } = new();
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class HistoricoStatusOutputDTO
{
    public string Status { get; set; } = string.Empty;
    public DateTime DataAlteracao { get; set; }
    public string? Observacao { get; set; }
}
