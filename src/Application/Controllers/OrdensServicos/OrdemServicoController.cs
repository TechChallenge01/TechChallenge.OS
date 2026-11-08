using Application.Gateways.OrdemServicos;
using Application.Interfaces;
using Application.Presenters.OrdensServicos;
using Application.UseCases.OrdensServicos;
using Domain.Enums;
using Shared.DTOs.OrdemServicos.Request;
using Shared.Result;

namespace Application.Controllers.OrdensServicos;

public class OrdemServicoController
{
    private readonly IOrdemServicoDataSource _dataSource;

    public OrdemServicoController(IOrdemServicoDataSource dataSource)
        => _dataSource = dataSource;

    public async Task<ICommandResult> Abrir(
        AbrirOrdemServicoRequestDTO request, Guid idUsuario,
        IEventPublisher eventPublisher, CancellationToken ct, IMetricsService? metrics = null)
    {
        var presenter = new OrdemServicoPresenter("Ordem de serviço aberta com sucesso.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = AbrirOrdemServicoUseCase.Create(gateway, eventPublisher);
            var id = await useCase.Run(request, idUsuario, ct);
            metrics?.Increment("os.aberta", new[] { "servico:os" });
            return presenter.Created(id);
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> GetById(Guid id, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Ordem de serviço encontrada.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = GetByIdUseCase.Create(gateway);
            var os = await useCase.Run(id, ct);
            if (os is null) return presenter.NotFound("Ordem de serviço não encontrada.");
            return presenter.TransformObject(os);
        }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Lista de ordens de serviço.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = GetPaginatedUseCase.Create(gateway);
            var (items, total) = await useCase.Run(page, pageSize, ct);
            return presenter.TransformPaged(items, page, total, pageSize);
        }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> IniciarDiagnostico(Guid ordemServicoId, Guid idUsuario, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Diagnóstico iniciado.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = IniciarDiagnosticoUseCase.Create(gateway);
            await useCase.Run(ordemServicoId, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> RegistrarDiagnostico(
        Guid ordemServicoId, RegistrarDiagnosticoRequestDTO request,
        Guid idUsuario, IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Diagnóstico registrado com sucesso.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = RegistrarDiagnosticoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(ordemServicoId, request, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> Cancelar(
        Guid ordemServicoId, CancelarOrdemServicoRequestDTO request,
        Guid idUsuario, IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Ordem de serviço cancelada.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = CancelarOrdemServicoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(ordemServicoId, request.Motivo, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> AtualizarStatus(
        Guid ordemServicoId, EStatusOS novoStatus, Guid idUsuario, string? observacao, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Status atualizado.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = AtualizarStatusUseCase.Create(gateway);
            await useCase.Run(ordemServicoId, novoStatus, idUsuario, observacao, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }

    public async Task<ICommandResult> Finalizar(
        Guid ordemServicoId, decimal valorFinal, Guid idUsuario,
        IEventPublisher eventPublisher, CancellationToken ct)
    {
        var presenter = new OrdemServicoPresenter("Ordem de serviço finalizada.");
        try
        {
            var gateway = OrdemServicoGateway.Create(_dataSource);
            var useCase = FinalizarOrdemServicoUseCase.Create(gateway, eventPublisher);
            await useCase.Run(ordemServicoId, valorFinal, idUsuario, ct);
            return presenter.NoContent();
        }
        catch (KeyNotFoundException ex) { return presenter.NotFound(ex.Message); }
        catch (InvalidOperationException ex) { return presenter.BadRequest(ex.Message); }
        catch (Exception ex) { return presenter.InternalError(ex.Message); }
    }
}
