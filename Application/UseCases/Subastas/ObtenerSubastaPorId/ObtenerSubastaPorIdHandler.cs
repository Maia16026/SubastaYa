using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using System.Linq;

namespace Application.UseCases.Subastas.ObtenerSubastaPorId;

public class ObtenerSubastaPorIdHandler
    : IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDto?>
{
    private readonly ISubastaRepository _repository;

    public ObtenerSubastaPorIdHandler(ISubastaRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubastaDto?> Handle(
        ObtenerSubastaPorIdQuery query,
        CancellationToken ct = default)
    {
        var subasta = await _repository.ObtenerConPujasAsync(query.Id, ct);

        if (subasta is null)
            return null;

        return new SubastaDto
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
            PujaActual = subasta.Pujas.Any() ? subasta.Pujas.Max(p => p.Monto) : null,
            CantidadPujas = subasta.Pujas.Count
        };
    }
}