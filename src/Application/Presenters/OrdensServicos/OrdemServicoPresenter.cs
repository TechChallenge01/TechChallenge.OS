using System.Net;
using Domain.Aggregates.OrdemServicoAggregates;
using Shared.DTOs;
using Shared.DTOs.OrdemServicos.Output;
using Shared.Result;

namespace Application.Presenters.OrdensServicos;

public class OrdemServicoPresenter
{
    private readonly string _message;

    public OrdemServicoPresenter(string message) => _message = message;

    public ICommandResult<OrdemServicoOutputDTO> TransformObject(OrdemServico ordemServico)
        => new CommandResult<OrdemServicoOutputDTO>(_message, HttpStatusCode.OK, Transform(ordemServico));

    public ICommandResult<List<OrdemServicoOutputDTO>> TransformList(List<OrdemServico> ordensServico)
        => new CommandResult<List<OrdemServicoOutputDTO>>(_message, HttpStatusCode.OK, ordensServico.Select(Transform).ToList());

    public ICommandResult<PagedResultDTO<OrdemServicoOutputDTO>> TransformPaged(List<OrdemServico> items, int page, int total, int pageSize = 10)
        => new CommandResult<PagedResultDTO<OrdemServicoOutputDTO>>(_message, HttpStatusCode.PartialContent,
            new PagedResultDTO<OrdemServicoOutputDTO>
            {
                Items = items.Select(Transform).ToList(),
                Total = total,
                Page = page,
                PageSize = pageSize
            });

    public ICommandResult<T> Created<T>(T data)
        => new CommandResult<T>(_message, HttpStatusCode.Created, data);

    public ICommandResult NoContent()
        => new CommandResult(_message, HttpStatusCode.NoContent);

    public ICommandResult NotFound(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.NotFound);

    public ICommandResult BadRequest(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.BadRequest);

    public ICommandResult InternalError(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.InternalServerError);

    public ICommandResult Forbidden(string? message = null)
        => new CommandResult(message ?? _message, HttpStatusCode.Forbidden);

    private static OrdemServicoOutputDTO Transform(OrdemServico os) => new()
    {
        Id = os.Id,
        ClienteId = os.ClienteId,
        VeiculoId = os.VeiculoId,
        DescricaoProblema = os.DescricaoProblema,
        Status = os.Status.ToString(),
        ValorEstimado = os.ValorEstimado,
        ValorFinal = os.ValorFinal,
        Diagnostico = os.Diagnostico,
        Historico = os.Historico.Select(h => new HistoricoStatusOutputDTO
        {
            Status = h.Status,
            DataAlteracao = h.DataAlteracao,
            Observacao = h.Observacao
        }).ToList(),
        DataCriacao = os.DataCriacao,
        DataAtualizacao = os.DataAtualizacao
    };
}
