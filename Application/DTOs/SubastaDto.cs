namespace Application.DTOs;

public class SubastaDto
{
    public int Id { get; set; }
    public int VendedorId { get; set; }
    public string Titulo { get; set; } = null!;
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = null!;
    public int? GanadorId { get; set; }
}