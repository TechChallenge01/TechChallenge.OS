namespace Domain.BaseEntity;

public abstract class Base
{
    protected Base(Guid idUsuarioCriacao, DateTime dataCriacao, Guid? idUsuarioAtualizacao = null, DateTime? dataAtualizacao = null)
    {
        IdUsuarioCriacao = idUsuarioCriacao;
        DataCriacao = dataCriacao;
        IdUsuarioAtualizacao = idUsuarioAtualizacao;
        DataAtualizacao = dataAtualizacao;
    }

    public Guid IdUsuarioCriacao { get; protected set; }
    public DateTime DataCriacao { get; protected set; }
    public Guid? IdUsuarioAtualizacao { get; protected set; }
    public DateTime? DataAtualizacao { get; protected set; }
    public bool Ativo { get; protected set; } = true;

    public void Inativar() => Ativo = false;

    public void RastrearAlteracao(Guid idUsuarioAtualizacao, DateTime dataAtualizacao)
    {
        IdUsuarioAtualizacao = idUsuarioAtualizacao;
        DataAtualizacao = dataAtualizacao;
    }
}
