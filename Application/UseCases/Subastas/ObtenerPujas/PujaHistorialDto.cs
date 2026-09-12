using System;

namespace Application.UseCases.Subastas.ObtenerPujas;

public class PujaHistorialDto
{
    public string Seudonimo { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }
}
