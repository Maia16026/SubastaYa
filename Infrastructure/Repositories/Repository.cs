using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class Repository<T> : IRepository<T>
    where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> ObtenerAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<List<T>> ListarAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
    }

    public void Actualizar(T entidad)
    {
        _dbSet.Update(entidad);
    }

    public void Eliminar(T entidad)
    {
        _dbSet.Remove(entidad);
    }
}