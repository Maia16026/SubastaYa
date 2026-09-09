namespace Application.DTOs;

public class TransaccionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = null!;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public int? SubastaId { get; set; }
}
