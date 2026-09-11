using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using System.Linq;

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
        var subastas = await _repository.ListarConPujasAsync(
            query.Estado,
            query.CategoriaId,
            query.PrecioMin,
            query.PrecioMax,
            query.Orden,
            ct);

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
