using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;

namespace API.EndPoints.OrdemServicos;

public class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/ordens-servico/{id:guid}", async (
            Guid id,
            IOrdemServicoDataSource dataSource,
            CancellationToken ct) =>
        {
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.GetById(id, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Busca uma ordem de serviço por ID");
    }
}
