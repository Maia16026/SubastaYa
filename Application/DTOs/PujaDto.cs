namespace Application.DTOs;

public class PujaDto
{
    public int Id { get; set; }
    public int SubastaId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }
}