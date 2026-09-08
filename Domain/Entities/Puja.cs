using System;

namespace Domain.Entities;

public class Puja
{
    public int Id { get; private set; }

    public int SubastaId { get; private set; }

    public int CompradorId { get; private set; }

    public decimal Monto { get; private set; }

    public DateTime FechaPuja { get; private set; }

    // Navegación
    public Subasta Subasta { get; private set; } = null!;
    public Usuario Comprador { get; private set; } = null!;

    protected Puja() { }

    public Puja(int subastaId, int compradorId, decimal monto, DateTime fechaPuja)
    {
        SubastaId = subastaId;
        CompradorId = compradorId;
        Monto = monto;
        FechaPuja = fechaPuja;
    }
}
