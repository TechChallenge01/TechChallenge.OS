namespace Shared.DTOs.OrdemServicos.Request;

public class AbrirOrdemServicoRequestDTO
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public string DescricaoProblema { get; set; } = string.Empty;
}

public class RegistrarDiagnosticoRequestDTO
{
    public string Diagnostico { get; set; } = string.Empty;
    public decimal ValorEstimado { get; set; }
}

public class CancelarOrdemServicoRequestDTO
{
    public string Motivo { get; set; } = string.Empty;
}
