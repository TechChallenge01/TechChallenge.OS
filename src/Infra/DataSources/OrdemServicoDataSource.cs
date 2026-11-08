using Application.Interfaces;
using MongoDB.Driver;
using Shared.DTOs.OrdemServicos.Input;

namespace Infra.DataSources;

public class OrdemServicoDataSource : IOrdemServicoDataSource
{
    private readonly IMongoCollection<OrdemServicoInputDTO> _collection;

    public OrdemServicoDataSource(IMongoDatabase database)
        => _collection = database.GetCollection<OrdemServicoInputDTO>("ordens_servico");

    public async Task<OrdemServicoInputDTO?> GetById(Guid id, CancellationToken ct)
        => await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);

    public async Task<List<OrdemServicoInputDTO>> GetByClienteId(Guid clienteId, CancellationToken ct)
        => await _collection.Find(x => x.ClienteId == clienteId).ToListAsync(ct);

    public async Task<List<OrdemServicoInputDTO>> GetByStatus(string status, CancellationToken ct)
        => await _collection.Find(x => x.Status == status).ToListAsync(ct);

    public async Task<(List<OrdemServicoInputDTO>, int)> GetPaginated(int page, int pageSize, CancellationToken ct)
    {
        var filter = Builders<OrdemServicoInputDTO>.Filter.Empty;
        var total = (int)await _collection.CountDocumentsAsync(filter, cancellationToken: ct);
        var items = await _collection.Find(filter)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task Create(OrdemServicoInputDTO dto, CancellationToken ct)
        => await _collection.InsertOneAsync(dto, cancellationToken: ct);

    public async Task Update(OrdemServicoInputDTO dto, CancellationToken ct)
        => await _collection.ReplaceOneAsync(x => x.Id == dto.Id, dto, cancellationToken: ct);
}
