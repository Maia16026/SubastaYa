using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Subasta
{
    public int Id { get; private set; }

    public int VendedorId { get; private set; }

    public string Titulo { get; private set; }= null!;

    public decimal PrecioBase { get; private set; }

    public decimal IncrementoMinimo { get; private set; }

    public DateTime FechaInicio { get; private set; }

    public DateTime FechaFin { get; private set; }

    public EstadoSubasta Estado { get; private set; }

    public int? GanadorId { get; private set; }

    public byte[] RowVersion { get; private set; } = null!;

    protected Subasta() { }

    public Subasta(
        int vendedorId,
        string titulo,
        decimal precioBase,
        decimal incrementoMinimo,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
        if (vendedorId <= 0)
            throw new DomainException("El vendedor es obligatorio.");

        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("El título es obligatorio.");

        if (precioBase <= 0)
            throw new DomainException(
                "El precio base debe ser mayor a cero.");

        if (incrementoMinimo <= 0)
            throw new DomainException(
                "El incremento mínimo debe ser mayor a cero.");

        if (fechaFin <= fechaInicio)
            throw new DomainException(
                "La fecha de fin debe ser posterior a la fecha de inicio.");

        VendedorId = vendedorId;
        Titulo = titulo;
        PrecioBase = precioBase;
        IncrementoMinimo = incrementoMinimo;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;

        Estado = fechaInicio > DateTime.UtcNow
            ? EstadoSubasta.Proxima
            : EstadoSubasta.Activa;
    }

    public bool PuedeRecibirPuja(decimal monto, decimal? pujaActual)
    {
        if (Estado != EstadoSubasta.Activa)
            return false;

        if (pujaActual is null)
            return monto >= PrecioBase;

        return monto >= pujaActual.Value + IncrementoMinimo;
    }
}