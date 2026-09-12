namespace Application.UseCases.Subastas.ObtenerSubastasPorVendedor;

public class PublicacionVendedorDto
{
    public int SubastaId { get; set; }
    public string Titulo { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public decimal? PujaActual { get; set; }
    public int CantidadPujas { get; set; }
    public decimal? MontoAdjudicado { get; set; }
}
