using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Application.UseCases.Subastas.ObtenerSubastas;

public class ObtenerSubastasHandler
    : IQueryHandler<ObtenerSubastasQuery, SubastasPaginadasDto>
{
    private readonly ISubastaRepository _repository;

    public ObtenerSubastasHandler(ISubastaRepository repository)
    {
        _repository = repository;
    }

    public async Task<SubastasPaginadasDto> Handle(
        ObtenerSubastasQuery query,
        CancellationToken ct = default)
    {
        // Solicitar al repositorio la página solicitada y el total filtrado
        var result = await _repository.ListarConPujasPaginadasAsync(
            query.Estado,
            query.CategoriaId,
            query.PrecioMin,
            query.PrecioMax,
            query.Orden,
            query.Pagina,
            query.TamanioPagina,
            ct);

        var items = result.Items;
        var total = result.TotalRegistros;

        // Mapear entidades a DTOs de forma explícita y clara
        var dtos = new List<SubastaDto>();

        foreach (var subasta in items)
        {
            var dto = new SubastaDto
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
            };

            dtos.Add(dto);
        }

        // Calcular total de páginas
        var totalPaginas = 0;

        if (query.TamanioPagina > 0)
        {
            totalPaginas =
                (int)System.Math.Ceiling(
                    (double)total / query.TamanioPagina);
        }

        return new SubastasPaginadasDto
        {
            Pagina = query.Pagina,
            TamanioPagina = query.TamanioPagina,
            TotalRegistros = total,
            TotalPaginas = totalPaginas,
            Items = dtos
        };
    }
}