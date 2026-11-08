using Application.Gateways.OrdemServicos;
using Application.Interfaces;

namespace Application.UseCases.OrdensServicos;

public class CancelarOrdemServicoUseCase
{
    private readonly OrdemServicoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private CancelarOrdemServicoUseCase(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static CancelarOrdemServicoUseCase Create(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid ordemServicoId, string motivo, Guid idUsuario, CancellationToken ct)
    {
        var os = await _gateway.GetById(ordemServicoId, ct)
            ?? throw new KeyNotFoundException($"OS {ordemServicoId} não encontrada.");

        os.Cancelar(idUsuario, motivo);
        await _gateway.Update(os, ct);

        await _eventPublisher.PublishAsync("os.cancelada", new
        {
            OrdemServicoId = os.Id,
            os.ClienteId,
            Motivo = motivo
        }, ct);
    }
}
