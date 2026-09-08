using System;
using Domain.Enums;

namespace Domain.Entities;

public class TransaccionLedger
{
    public int Id { get; private set; }

    public int BilleteraId { get; private set; }

    public TipoMovimiento Tipo { get; private set; }

    public decimal Monto { get; private set; }

    public DateTime Fecha { get; private set; }

    public int? SubastaId { get; private set; }

    // Navegación
    public Billetera Billetera { get; private set; } = null!;
    public Subasta? Subasta { get; private set; }

    protected TransaccionLedger() { }

    public TransaccionLedger(int billeteraId, TipoMovimiento tipo, decimal monto, DateTime fecha, int? subastaId = null)
    {
        BilleteraId = billeteraId;
        Tipo = tipo;
        Monto = monto;
        Fecha = fecha;
        SubastaId = subastaId;
    }
}
