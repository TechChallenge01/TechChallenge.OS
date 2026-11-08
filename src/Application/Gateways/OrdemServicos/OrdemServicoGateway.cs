using Application.Interfaces;
using Domain.Aggregates.OrdemServicoAggregates;
using Domain.Enums;
using Domain.ValueObjects;
using Shared.DTOs.OrdemServicos.Input;

namespace Application.Gateways.OrdemServicos;

public class OrdemServicoGateway
{
    private readonly IOrdemServicoDataSource _dataSource;

    private OrdemServicoGateway(IOrdemServicoDataSource dataSource) => _dataSource = dataSource;

    public static OrdemServicoGateway Create(IOrdemServicoDataSource dataSource) => new(dataSource);

    public async Task<OrdemServico?> GetById(Guid id, CancellationToken ct)
    {
        var dto = await _dataSource.GetById(id, ct);
        return dto is null ? null : Reconstituir(dto);
    }

    public async Task<(List<OrdemServico>, int)> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var (dtos, total) = await _dataSource.GetPaginated(page, pageSize, ct);
        return (dtos.Select(Reconstituir).ToList(), total);
    }

    public async Task Create(OrdemServico os, CancellationToken ct)
        => await _dataSource.Create(Serializar(os), ct);

    public async Task Update(OrdemServico os, CancellationToken ct)
        => await _dataSource.Update(Serializar(os), ct);

    public async Task<List<OrdemServico>> GetByClienteId(Guid clienteId, CancellationToken ct)
    {
        var dtos = await _dataSource.GetByClienteId(clienteId, ct);
        return dtos.Select(Reconstituir).ToList();
    }

    private static OrdemServico Reconstituir(OrdemServicoInputDTO dto) =>
        new(
            dto.Id, dto.ClienteId, dto.VeiculoId, dto.DescricaoProblema,
            Enum.Parse<EStatusOS>(dto.Status),
            dto.ValorEstimado, dto.ValorFinal, dto.Diagnostico,
            dto.Historico.Select(h => new HistoricoStatus(h.Status, h.DataAlteracao, h.IdUsuario, h.Observacao)).ToList(),
            dto.IdUsuarioCriacao, dto.DataCriacao,
            dto.IdUsuarioAtualizacao, dto.DataAtualizacao, dto.Ativo
        );

    private static OrdemServicoInputDTO Serializar(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteId = os.ClienteId,
        VeiculoId = os.VeiculoId,
        DescricaoProblema = os.DescricaoProblema,
        Status = os.Status.ToString(),
        ValorEstimado = os.ValorEstimado,
        ValorFinal = os.ValorFinal,
        Diagnostico = os.Diagnostico,
        Historico = os.Historico.Select(h => new HistoricoStatusInputDTO
        {
            Status = h.Status,
            DataAlteracao = h.DataAlteracao,
            IdUsuario = h.IdUsuario,
            Observacao = h.Observacao
        }).ToList(),
        IdUsuarioCriacao = os.IdUsuarioCriacao,
        DataCriacao = os.DataCriacao,
        IdUsuarioAtualizacao = os.IdUsuarioAtualizacao,
        DataAtualizacao = os.DataAtualizacao,
        Ativo = os.Ativo
    };
}
