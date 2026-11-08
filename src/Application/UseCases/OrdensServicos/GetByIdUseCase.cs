using Application.Gateways.OrdemServicos;
using Domain.Aggregates.OrdemServicoAggregates;

namespace Application.UseCases.OrdensServicos;

public class GetByIdUseCase
{
    private readonly OrdemServicoGateway _gateway;

    private GetByIdUseCase(OrdemServicoGateway gateway) => _gateway = gateway;

    public static GetByIdUseCase Create(OrdemServicoGateway gateway) => new(gateway);

    public async Task<OrdemServico?> Run(Guid id, CancellationToken ct)
        => await _gateway.GetById(id, ct);
}
