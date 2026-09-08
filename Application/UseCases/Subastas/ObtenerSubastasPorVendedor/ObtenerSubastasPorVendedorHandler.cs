using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.UseCases.Subastas.ObtenerSubastasPorVendedor;

public class ObtenerSubastasPorVendedorHandler
{
    private readonly IRepository<Subasta> _repository;

    public ObtenerSubastasPorVendedorHandler(
        IRepository<Subasta> repository)
    {
        _repository = repository;
    }

    public async Task<List<SubastaDto>> Handle(
        ObtenerSubastasPorVendedorQuery query)
    {
        var subastas = await _repository.ListarAsync();

        return subastas
            .Where(s => s.VendedorId == query.VendedorId)
            .OrderByDescending(s => s.FechaInicio)
            .Select(s => new SubastaDto
            {
                Id = s.Id,
                VendedorId = s.VendedorId,
                CategoriaId = s.CategoriaId,
                Titulo = s.Titulo,
                Descripcion = s.Descripcion,
                UrlImagen = s.UrlImagen,
                PrecioBase = s.PrecioBase,
                IncrementoMinimo = s.IncrementoMinimo,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Estado = s.Estado.ToString()
            })
            .ToList();
    }
}