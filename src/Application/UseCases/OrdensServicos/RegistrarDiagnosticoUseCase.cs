using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Shared.DTOs.OrdemServicos.Request;

namespace Application.UseCases.OrdensServicos;

public class RegistrarDiagnosticoUseCase
{
    private readonly OrdemServicoGateway _gateway;
    private readonly IEventPublisher _eventPublisher;

    private RegistrarDiagnosticoUseCase(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
    {
        _gateway = gateway;
        _eventPublisher = eventPublisher;
    }

    public static RegistrarDiagnosticoUseCase Create(OrdemServicoGateway gateway, IEventPublisher eventPublisher)
        => new(gateway, eventPublisher);

    public async Task Run(Guid ordemServicoId, RegistrarDiagnosticoRequestDTO request, Guid idUsuario, CancellationToken ct)
    {
        var os = await _gateway.GetById(ordemServicoId, ct)
            ?? throw new KeyNotFoundException($"OS {ordemServicoId} não encontrada.");

        os.RegistrarDiagnostico(request.Diagnostico, request.ValorEstimado, idUsuario);
        await _gateway.Update(os, ct);

        await _eventPublisher.PublishAsync("diagnostico.concluido", new
        {
            OrdemServicoId = os.Id,
            os.ClienteId,
            os.Diagnostico,
            os.ValorEstimado
        }, ct);
    }
}
