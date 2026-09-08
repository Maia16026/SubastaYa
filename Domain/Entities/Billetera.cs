using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Billetera
{
    public int Id { get; private set; }

    public int UsuarioId { get; private set; }

    public decimal SaldoTotal { get; private set; }

    public decimal SaldoRetenido { get; private set; }

    public decimal SaldoDisponible { get; private set; }

    public int Version { get; private set; }

    // Navegación
    public Usuario Usuario { get; private set; } = null!;
    public ICollection<TransaccionLedger> Transacciones { get; private set; } = new List<TransaccionLedger>();

    protected Billetera() { }

    public Billetera(int usuarioId, decimal saldoTotal, decimal saldoRetenido, decimal saldoDisponible, int version)
    {
        UsuarioId = usuarioId;
        SaldoTotal = saldoTotal;
        SaldoRetenido = saldoRetenido;
        SaldoDisponible = saldoDisponible;
        Version = version;
    }
}
