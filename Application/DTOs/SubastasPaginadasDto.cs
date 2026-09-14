using System.Collections.Generic;

namespace Application.DTOs;

public class SubastasPaginadasDto
{
    public int Pagina { get; set; }
    public int TamanioPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public List<SubastaDto> Items { get; set; } = new List<SubastaDto>();
}