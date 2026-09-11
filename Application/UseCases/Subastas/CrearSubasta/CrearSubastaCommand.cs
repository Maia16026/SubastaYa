namespace Application.UseCases.Subastas.CrearSubasta;

public record CrearSubastaCommand(
    int VendedorId,
    int CategoriaId,
    string Titulo,
    string Descripcion,
    string UrlImagen,
    decimal PrecioBase,
    decimal IncrementoMinimo,
    DateTimeOffset FechaInicio,
    DateTimeOffset FechaFin);
