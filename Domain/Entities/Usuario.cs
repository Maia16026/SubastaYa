using System;
using System.Collections.Generic;

namespace Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = null!;

    public string Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public DateTime FechaRegistro { get; private set; }

    // Navegación
    public Billetera Billetera { get; private set; } = null!;
    public ICollection<Subasta> Subastas { get; private set; } = new List<Subasta>();
    public ICollection<Puja> Pujas { get; private set; } = new List<Puja>();
    public ICollection<AuditoriaLog> AuditoriaLogs { get; private set; } = new List<AuditoriaLog>();

    protected Usuario() { }

    public Usuario(string nombre, string email, string passwordHash, DateTime fechaRegistro)
    {
        Nombre = nombre;
        Email = email;
        PasswordHash = passwordHash;
        FechaRegistro = fechaRegistro;
    }
}
