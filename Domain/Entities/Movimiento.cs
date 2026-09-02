using System;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Movimiento
{
    public int Id { get; private set; }
    public int UsuarioId { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public decimal Monto { get; private set; }
    public DateTime FechaHora { get; private set; }

    protected Movimiento() { }

    public Movimiento(int usuarioId, TipoMovimiento tipo, decimal monto, DateTime fechaHora)
    {
        if (usuarioId <= 0) throw new DomainException("El usuario es obligatorio.");
        if (monto <= 0) throw new DomainException("El monto debe ser mayor a cero.");

        UsuarioId = usuarioId;
        Tipo = tipo;
        Monto = monto;
        FechaHora = fechaHora;
    }
}