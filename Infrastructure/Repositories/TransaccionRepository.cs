using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class TransaccionRepository : Repository<TransaccionLedger>, ITransaccionRepository
{
    public TransaccionRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<List<TransaccionLedger>> ListarPorBilleteraAsync(int billeteraId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(t => t.BilleteraId == billeteraId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync(ct);
    }
}
