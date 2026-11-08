using Application.Gateways.OrdemServicos;
using Domain.Aggregates.OrdemServicoAggregates;

namespace Application.UseCases.OrdensServicos;

public class GetPaginatedUseCase
{
    private readonly OrdemServicoGateway _gateway;

    private GetPaginatedUseCase(OrdemServicoGateway gateway) => _gateway = gateway;

    public static GetPaginatedUseCase Create(OrdemServicoGateway gateway) => new(gateway);

    public async Task<(List<OrdemServico> items, int total)> Run(int page, int pageSize, CancellationToken ct)
        => await _gateway.GetPaginated(page, pageSize, ct);
}
