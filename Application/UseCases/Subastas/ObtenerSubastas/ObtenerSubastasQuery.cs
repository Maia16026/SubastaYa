using Domain.Enums;

namespace Application.UseCases.Subastas.ObtenerSubastas;

public record ObtenerSubastasQuery(
    EstadoSubasta? Estado,
    int? CategoriaId,
    decimal? PrecioMin,
    decimal? PrecioMax,
    string? Orden);