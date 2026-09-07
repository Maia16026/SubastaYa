using Domain.Exceptions;

namespace Domain.Entities;

public class Puja
{
    public int Id { get; private set; }

    public int SubastaId { get; private set; }

    public int UsuarioId { get; private set; }

    public decimal Monto { get; private set; }

    public DateTime FechaHora { get; private set; }

    protected Puja() { }

    public Puja(
        int subastaId,
        int usuarioId,
        decimal monto,
        DateTime fechaHora)
    {
        if (subastaId <= 0)
            throw new DomainException("La subasta es obligatoria.");

        if (usuarioId <= 0)
            throw new DomainException("El usuario es obligatorio.");

        if (monto <= 0)
            throw new DomainException(
                "El monto debe ser mayor a cero.");

        SubastaId = subastaId;
        UsuarioId = usuarioId;
        Monto = monto;
        FechaHora = fechaHora;
    }
}