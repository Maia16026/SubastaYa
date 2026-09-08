using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.UseCases.Subastas.ObtenerSubastas;

public class ObtenerSubastasHandler
    : IQueryHandler<ObtenerSubastasQuery, SubastaDto?>
{
    private readonly IRepository<Subasta> _repository;

    public ObtenerSubastasHandler(IRepository<Subasta> repository)
    {
        _repository = repository;
    }

    public async Task<SubastaDto?> Handle(
        ObtenerSubastasQuery query,
        CancellationToken ct = default)
    {
        var subasta = await _repository.ObtenerAsync(query.Id);

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
            Estado = subasta.Estado.ToString()
        };
    }
}