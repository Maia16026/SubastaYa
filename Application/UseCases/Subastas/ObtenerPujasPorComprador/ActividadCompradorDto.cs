namespace Application.UseCases.Subastas.ObtenerPujasPorComprador;

public class ActividadCompradorDto
{
    public int SubastaId { get; set; }
    public string Titulo { get; set; } = null!;
    public bool SigueAbierta { get; set; }
    public bool? Gano { get; set; }
}
