using System.Collections.Generic;

namespace Domain.Entities;

public class Categoria
{
    public int Id { get; private set; }

    public string Nombre { get; private set; } = null!;

    public string UrlIcono { get; private set; } = null!;

    // Navegación
    public ICollection<Subasta> Subastas { get; private set; } = new List<Subasta>();

    protected Categoria() { }

    public Categoria(string nombre, string urlIcono)
    {
        Nombre = nombre;
        UrlIcono = urlIcono;
    }
}
