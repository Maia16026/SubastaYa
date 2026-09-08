using System;
using System.Collections.Generic;
using Domain.Exceptions;

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
    public void Retener(decimal monto)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        if (SaldoDisponible < monto)
            throw new DomainException("Saldo insuficiente.");

        SaldoDisponible -= monto;
        SaldoRetenido += monto;
    }

    public void Liberar(decimal monto)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        if (SaldoRetenido < monto)
            throw new DomainException("Saldo retenido insuficiente.");

        SaldoRetenido -= monto;
        SaldoDisponible += monto;
    }

    public void IncrementarVersion()
    {
        Version++;
    }
}
