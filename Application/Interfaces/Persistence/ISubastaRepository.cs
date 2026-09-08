using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ISubastaRepository : IRepository<Subasta>
{
    Task<List<Subasta>> ListarConPujasAsync();
}