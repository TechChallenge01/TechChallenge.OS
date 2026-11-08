using Application.Gateways.OrdemServicos;
using Application.Interfaces;

namespace Application.UseCases.OrdensServicos;

public class FinalizarOrdemServicoUseCase
{
    private readonly OrdemServicoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private FinalizarOrdemServicoUseCase(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static FinalizarOrdemServicoUseCase Create(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid ordemServicoId, decimal valorFinal, Guid idUsuario, CancellationToken ct)
    {
        var os = await _gateway.GetById(ordemServicoId, ct)
            ?? throw new KeyNotFoundException($"OS {ordemServicoId} não encontrada.");

        os.Finalizar(valorFinal, idUsuario);
        await _gateway.Update(os, ct);

        await _eventPublisher.PublishAsync("execucao.finalizada", new
        {
            OrdemServicoId = os.Id,
            os.ClienteId,
            ValorFinal = valorFinal
        }, ct);
    }
}
