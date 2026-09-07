using Domain.Exceptions;

namespace Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Email { get; private set; }

    public decimal SaldoTotal { get; private set; }

    public decimal SaldoRetenido { get; private set; }

    public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

    protected Usuario() { }

    public Usuario(string nombre, string email)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El email es obligatorio.");

        Nombre = nombre;
        Email = email;
        SaldoTotal = 0m;
        SaldoRetenido = 0m;
    }

    public void AcreditarSaldo(decimal monto)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        SaldoTotal += monto;
    }

    public void RetenerFondos(decimal monto)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        if (monto > SaldoDisponible)
            throw new DomainException("Saldo insuficiente.");

        SaldoRetenido += monto;
    }

    public void LiberarFondos(decimal monto)
    {
        if (monto <= 0)
            throw new DomainException("El monto debe ser mayor a cero.");

        if (monto > SaldoRetenido)
            throw new DomainException(
                "No se puede liberar más saldo del retenido.");

        SaldoRetenido -= monto;
    }
}