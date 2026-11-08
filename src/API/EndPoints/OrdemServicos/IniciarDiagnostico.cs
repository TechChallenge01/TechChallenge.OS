using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;

namespace API.EndPoints.OrdemServicos;

public class IniciarDiagnostico : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/ordens-servico/{id:guid}/iniciar-diagnostico", async (
            Guid id,
            HttpContext ctx,
            IOrdemServicoDataSource dataSource,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.IniciarDiagnostico(id, idUsuario, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Inicia o diagnóstico de uma OS");
    }
}
