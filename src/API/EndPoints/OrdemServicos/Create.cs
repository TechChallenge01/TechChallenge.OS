using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.OrdemServicos.Request;

namespace API.EndPoints.OrdemServicos;

public class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/ordens-servico", async (
            [FromBody] AbrirOrdemServicoRequestDTO request,
            HttpContext ctx,
            IOrdemServicoDataSource dataSource,
            IEventPublisher eventPublisher,
            IMetricsService metrics,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.Abrir(request, idUsuario, eventPublisher, ct, metrics);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Abre uma nova ordem de serviço");
    }
}
