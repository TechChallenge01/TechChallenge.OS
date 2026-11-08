using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.OrdemServicos.Request;

namespace API.EndPoints.OrdemServicos;

public class Cancelar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/ordens-servico/{id:guid}/cancelar", async (
            Guid id,
            [FromBody] CancelarOrdemServicoRequestDTO request,
            HttpContext ctx,
            IOrdemServicoDataSource dataSource,
            IEventPublisher eventPublisher,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.Cancelar(id, request, idUsuario, eventPublisher, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Cancela uma ordem de serviço");
    }
}
