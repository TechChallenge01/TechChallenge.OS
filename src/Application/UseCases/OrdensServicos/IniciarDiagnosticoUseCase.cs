using Application.Gateways.OrdemServicos;

namespace Application.UseCases.OrdensServicos;

public class IniciarDiagnosticoUseCase
{
    private readonly OrdemServicoGateway _gateway;

    private IniciarDiagnosticoUseCase(OrdemServicoGateway gateway) => _gateway = gateway;

    public static IniciarDiagnosticoUseCase Create(OrdemServicoGateway gateway) => new(gateway);

    public async Task Run(Guid ordemServicoId, Guid idUsuario, CancellationToken ct)
    {
        var os = await _gateway.GetById(ordemServicoId, ct)
            ?? throw new KeyNotFoundException($"OS {ordemServicoId} não encontrada.");

        os.IniciarDiagnostico(idUsuario);
        await _gateway.Update(os, ct);
    }
}
