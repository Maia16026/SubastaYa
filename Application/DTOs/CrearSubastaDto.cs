namespace Application.DTOs;

public class CrearSubastaDto
{
    public int VendedorId { get; set; }
    public string Titulo { get; set; } = null!;
    public string Descripcion { get; set; } = null!;
    public string Categoria { get; set; } = null!;
    public string UrlImagen { get; set; } = null!;
    public decimal PrecioBase { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}