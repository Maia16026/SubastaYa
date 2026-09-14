using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces.Persistence;

public interface ISubastaRepository : IRepository<Subasta>
{
    Task<List<Subasta>> ListarConPujasAsync(
        EstadoSubasta? estado,
        int? categoriaId,
        decimal? precioMin,
        decimal? precioMax,
        string? orden,
        CancellationToken ct = default);

    Task<(List<Subasta> Items, int TotalRegistros)> ListarConPujasPaginadasAsync(
        EstadoSubasta? estado,
        int? categoriaId,
        decimal? precioMin,
        decimal? precioMax,
        string? orden,
        int pagina,
        int tamanioPagina,
        CancellationToken ct = default);

    Task<Subasta?> ObtenerConPujasAsync(
        int id,
        CancellationToken ct = default);

    Task<List<Subasta>> ListarProgramadasParaActivarAsync(
        DateTime ahora,
        CancellationToken ct = default);

    Task<List<Subasta>> ListarActivasVencidasConPujasAsync(
        DateTime ahora,
        CancellationToken ct = default);
}