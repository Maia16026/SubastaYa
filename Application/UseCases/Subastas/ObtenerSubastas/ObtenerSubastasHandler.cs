using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.UseCases.Subastas.ObtenerSubastas;

public class ObtenerSubastasHandler
    : IQueryHandler<ObtenerSubastasQuery, List<SubastaDto>>
{
    private readonly ISubastaRepository _repository;

    public ObtenerSubastasHandler(ISubastaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<SubastaDto>> Handle(
        ObtenerSubastasQuery query,
        CancellationToken ct = default)
    {
        var subastas = await _repository.ListarConPujasAsync();

        if (query.Estado.HasValue)
        {
            subastas = subastas
                .Where(s => s.Estado == query.Estado.Value)
                .ToList();
        }

        if (query.CategoriaId.HasValue)
        {
            subastas = subastas
                .Where(s => s.CategoriaId == query.CategoriaId.Value)
                .ToList();
        }

        if (query.PrecioMin.HasValue)
        {
            subastas = subastas
                .Where(s => s.PrecioBase >= query.PrecioMin.Value)
                .ToList();
        }

        if (query.PrecioMax.HasValue)
        {
            subastas = subastas
                .Where(s => s.PrecioBase <= query.PrecioMax.Value)
                .ToList();
        }

        if (query.Orden?.ToLower() == "tiempo")
        {
            subastas = subastas
                .OrderBy(s => s.FechaFin)
                .ToList();
        }

        if (query.Orden?.ToLower() == "puja")
        {
            subastas = subastas
                .OrderByDescending(s =>
                    s.Pujas.Any()
                        ? s.Pujas.Max(p => p.Monto)
                        : 0)
                .ToList();
        }

        return subastas.Select(subasta => new SubastaDto
        {
            Id = subasta.Id,
            VendedorId = subasta.VendedorId,
            CategoriaId = subasta.CategoriaId,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            UrlImagen = subasta.UrlImagen,
            PrecioBase = subasta.PrecioBase,
            IncrementoMinimo = subasta.IncrementoMinimo,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Estado = subasta.Estado.ToString(),

            PujaActual = subasta.Pujas.Any()
                ? subasta.Pujas.Max(p => p.Monto)
                : null,

            CantidadPujas = subasta.Pujas.Count
        }).ToList();
    }
}