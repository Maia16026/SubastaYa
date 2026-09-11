using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IBilleteraRepository : IRepository<Billetera>
{
    Task<Billetera?> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default);
}
