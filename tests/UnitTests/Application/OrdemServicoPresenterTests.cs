using Application.Presenters.OrdensServicos;
using Domain.Aggregates.OrdemServicoAggregates;
using FluentAssertions;
using System.Net;
using Xunit;

namespace UnitTests.Application;

public class OrdemServicoPresenterTests
{
    private static OrdemServico CriarOS() => new(Guid.NewGuid(), Guid.NewGuid(), "Problema", Guid.NewGuid());
    private static OrdemServicoPresenter CriarPresenter() => new("Operação realizada");

    [Fact]
    public void TransformObject_DeveRetornarStatusOK()
    {
        var result = CriarPresenter().TransformObject(CriarOS());
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public void TransformList_DeveRetornarLista()
    {
        var list = new List<OrdemServico> { CriarOS(), CriarOS() };
        var result = CriarPresenter().TransformList(list);
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public void TransformPaged_DeveRetornarPartialContent()
    {
        var list = new List<OrdemServico> { CriarOS() };
        var result = CriarPresenter().TransformPaged(list, 1, 10);
        result.StatusCode.Should().Be(HttpStatusCode.PartialContent);
        result.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Created_DeveRetornarStatusCreated()
    {
        var result = CriarPresenter().Created(Guid.NewGuid());
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public void NoContent_DeveRetornarStatusNoContent()
    {
        var result = CriarPresenter().NoContent();
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public void NotFound_DeveRetornarStatusNotFound()
    {
        var result = CriarPresenter().NotFound("não encontrado");
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public void BadRequest_DeveRetornarStatusBadRequest()
    {
        var result = CriarPresenter().BadRequest("erro");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public void InternalError_DeveRetornarStatus500()
    {
        var result = CriarPresenter().InternalError();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public void Forbidden_DeveRetornarStatusForbidden()
    {
        var result = CriarPresenter().Forbidden();
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
