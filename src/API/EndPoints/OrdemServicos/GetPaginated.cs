using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.EndPoints.OrdemServicos;

public class GetPaginated : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/ordens-servico", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IOrdemServicoDataSource dataSource,
            CancellationToken ct) =>
        {
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.GetPaginated(page, pageSize, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Lista ordens de serviço paginadas");
    }
}
