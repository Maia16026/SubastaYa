namespace Application.UseCases.Subastas.Pujar;

public record CrearPujaCommand(
    int SubastaId,
    int CompradorId,
    decimal Monto);