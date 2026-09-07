namespace Application.UseCases.Subastas.CrearSubasta;

public record CrearSubastaCommand(
    int VendedorId,
    string Titulo,
    decimal PrecioBase,
    decimal IncrementoMinimo,
    DateTime FechaInicio,
    DateTime FechaFin);