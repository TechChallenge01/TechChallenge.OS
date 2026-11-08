using API.Extensions;
using Application.Controllers.OrdensServicos;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.OrdemServicos.Request;

namespace API.EndPoints.OrdemServicos;

public class RegistrarDiagnostico : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/ordens-servico/{id:guid}/registrar-diagnostico", async (
            Guid id,
            [FromBody] RegistrarDiagnosticoRequestDTO request,
            HttpContext ctx,
            IOrdemServicoDataSource dataSource,
            IEventPublisher eventPublisher,
            CancellationToken ct) =>
        {
            var idUsuario = ctx.User.GetUserId();
            var controller = new OrdemServicoController(dataSource);
            var result = await controller.RegistrarDiagnostico(id, request, idUsuario, eventPublisher, ct);
            return result.ToMinimalApiResult();
        })
        .RequireAuthorization()
        .WithTags("Ordens de Serviço")
        .WithSummary("Registra o diagnóstico e valor estimado de uma OS");
    }
}
