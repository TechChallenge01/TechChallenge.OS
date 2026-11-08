using Application.Gateways.OrdemServicos;
using Domain.Enums;

namespace Application.UseCases.OrdensServicos;

public class AtualizarStatusUseCase
{
    private readonly OrdemServicoGateway _gateway;

    private AtualizarStatusUseCase(OrdemServicoGateway gateway) => _gateway = gateway;

    public static AtualizarStatusUseCase Create(OrdemServicoGateway gateway) => new(gateway);

    public async Task Run(Guid ordemServicoId, EStatusOS novoStatus, Guid idUsuario, string? observacao, CancellationToken ct)
    {
        var os = await _gateway.GetById(ordemServicoId, ct)
            ?? throw new KeyNotFoundException($"OS {ordemServicoId} não encontrada.");

        switch (novoStatus)
        {
            case EStatusOS.EmExecucao:
                os.IniciarExecucao(idUsuario);
                break;
            case EStatusOS.Finalizada:
                throw new InvalidOperationException("Para finalizar use o endpoint específico com valor final.");
            case EStatusOS.Entregue:
                os.RegistrarEntrega(idUsuario);
                break;
            case EStatusOS.Cancelada:
                os.Cancelar(idUsuario, observacao ?? "Cancelado pelo sistema");
                break;
            default:
                throw new InvalidOperationException($"Transição para {novoStatus} não disponível neste endpoint.");
        }

        await _gateway.Update(os, ct);
    }
}
