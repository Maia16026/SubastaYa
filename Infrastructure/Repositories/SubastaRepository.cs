using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
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

    public async Task<List<Subasta>> ListarConPujasAsync(
        EstadoSubasta? estado,
        int? categoriaId,
        decimal? precioMin,
        decimal? precioMax,
        string? orden,
        CancellationToken ct = default)
    {
        var query = _context.Subastas
            .Include(s => s.Pujas)
            .AsQueryable();

        // Aplicar filtros
        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == categoriaId.Value);
        }

        if (precioMin.HasValue)
        {
            query = query.Where(s => s.PrecioBase >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            query = query.Where(s => s.PrecioBase <= precioMax.Value);
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(orden))
        {
            var o = orden.ToLowerInvariant();

            if (o == "tiempo")
            {
                query = query
                    .OrderBy(s => s.FechaFin)
                    .ThenBy(s => s.Id);
            }
            else if (o == "puja")
            {
                query = query
                    .OrderByDescending(s =>
                        s.Pujas.Select(p => (decimal?)p.Monto).Max() ?? 0)
                    .ThenBy(s => s.Id);
            }
        }
        else
        {
            query = query.OrderBy(s => s.Id);
        }

        // Obtener todas las subastas que cumplen los filtros
        var items = await query.ToListAsync(ct);

        return items;
    }

    public async Task<(List<Subasta> Items, int TotalRegistros)> ListarConPujasPaginadasAsync(
        EstadoSubasta? estado,
        int? categoriaId,
        decimal? precioMin,
        decimal? precioMax,
        string? orden,
        int pagina,
        int tamanioPagina,
        CancellationToken ct = default)
    {
        var query = _context.Subastas
            .Include(s => s.Pujas)
            .AsQueryable();

        // Aplicar filtros
        if (estado.HasValue)
        {
            query = query.Where(s => s.Estado == estado.Value);
        }

        if (categoriaId.HasValue)
        {
            query = query.Where(s => s.CategoriaId == categoriaId.Value);
        }

        if (precioMin.HasValue)
        {
            query = query.Where(s => s.PrecioBase >= precioMin.Value);
        }

        if (precioMax.HasValue)
        {
            query = query.Where(s => s.PrecioBase <= precioMax.Value);
        }

        // Aplicar ordenamiento
        if (!string.IsNullOrWhiteSpace(orden))
        {
            var o = orden.ToLowerInvariant();

            if (o == "tiempo")
            {
                query = query
                    .OrderBy(s => s.FechaFin)
                    .ThenBy(s => s.Id);
            }
            else if (o == "puja")
            {
                query = query
                    .OrderByDescending(s =>
                        s.Pujas.Select(p => (decimal?)p.Monto).Max() ?? 0)
                    .ThenBy(s => s.Id);
            }
        }
        else
        {
            query = query.OrderBy(s => s.Id);
        }

        // Obtener la cantidad total de registros antes de paginar
        var total = await query.CountAsync(ct);

        // Obtener solamente los registros de la página solicitada
        var items = await query
            .Skip((pagina - 1) * tamanioPagina)
            .Take(tamanioPagina)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Subasta?> ObtenerConPujasAsync(
        int id,
        CancellationToken ct = default)
    {
        return await _context.Subastas
            .Include(s => s.Pujas)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }
}