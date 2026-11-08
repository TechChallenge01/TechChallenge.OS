using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Domain.Aggregates.OrdemServicoAggregates;
using Shared.DTOs.OrdemServicos.Request;

namespace Application.UseCases.OrdensServicos;

public class AbrirOrdemServicoUseCase
{
    private readonly OrdemServicoGateway _ordemServicoGateway;
    private readonly IEventPublisher _eventPublisher;

    private AbrirOrdemServicoUseCase(OrdemServicoGateway ordemServicoGateway, IEventPublisher eventPublisher)
    {
        _ordemServicoGateway = ordemServicoGateway;
        _eventPublisher = eventPublisher;
    }

    public static AbrirOrdemServicoUseCase Create(OrdemServicoGateway ordemServicoGateway, IEventPublisher eventPublisher)
        => new(ordemServicoGateway, eventPublisher);

    public async Task<Guid> Run(AbrirOrdemServicoRequestDTO request, Guid idUsuario, CancellationToken ct)
    {
        var os = new OrdemServico(request.ClienteId, request.VeiculoId, request.DescricaoProblema, idUsuario);

        await _ordemServicoGateway.Create(os, ct);

        await _eventPublisher.PublishAsync("os.aberta", new
        {
            OrdemServicoId = os.Id,
            os.ClienteId,
            os.VeiculoId,
            os.DescricaoProblema,
            DataCriacao = os.DataCriacao
        }, ct);

        return os.Id;
    }
}
