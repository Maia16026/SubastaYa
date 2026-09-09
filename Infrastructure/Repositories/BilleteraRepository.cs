using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class BilleteraRepository : Repository<Billetera>, IBilleteraRepository
{
    public BilleteraRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<Billetera?> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken ct = default)
    {
        return await _dbSet.SingleOrDefaultAsync(b => b.UsuarioId == usuarioId, ct);
    }
}
