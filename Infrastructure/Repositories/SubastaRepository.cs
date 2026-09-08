using Application.Interfaces.Persistence;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class SubastaRepository
    : Repository<Subasta>, ISubastaRepository
{
    public SubastaRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<List<Subasta>> ListarConPujasAsync()
    {
        return await _context.Subastas
            .Include(s => s.Pujas)
            .ToListAsync();
    }
}