using Shared.DTOs.OrdemServicos.Input;

namespace Application.Interfaces;

public interface IOrdemServicoDataSource
{
    Task<(List<OrdemServicoInputDTO> ordensServico, int total)> GetPaginated(int page, int pageSize, CancellationToken ct);
    Task<OrdemServicoInputDTO?> GetById(Guid id, CancellationToken ct);
    Task Create(OrdemServicoInputDTO ordemServico, CancellationToken ct);
    Task Update(OrdemServicoInputDTO ordemServico, CancellationToken ct);
    Task<List<OrdemServicoInputDTO>> GetByClienteId(Guid clienteId, CancellationToken ct);
    Task<List<OrdemServicoInputDTO>> GetByStatus(string status, CancellationToken ct);
}
