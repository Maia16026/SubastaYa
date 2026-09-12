using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Subastas.ObtenerPujasPorComprador;

public class ObtenerPujasPorCompradorHandler
{
    private readonly IRepository<Puja> _pujaRepository;
    private readonly ISubastaRepository _subastaRepository;

    public ObtenerPujasPorCompradorHandler(
        IRepository<Puja> pujaRepository,
        ISubastaRepository subastaRepository)
    {
        _pujaRepository = pujaRepository;
        _subastaRepository = subastaRepository;
    }

    public async Task<List<ActividadCompradorDto>> Handle(
        ObtenerPujasPorCompradorQuery query)
    {
        // 1. Obtener todas las pujas
        var todasLasPujas = await _pujaRepository.ListarAsync();

        // 2. Filtrar las del comprador
        var pujasDelComprador = todasLasPujas
            .Where(p => p.CompradorId == query.CompradorId)
            .ToList();

        // 3. Obtener SubastaId distintos
        var subastaIds = pujasDelComprador
            .Select(p => p.SubastaId)
            .Distinct()
            .ToList();

        // 4. Crear la lista resultado
        var resultado = new List<ActividadCompradorDto>();

        // 5. Recorrer los IDs con foreach
        foreach (var subastaId in subastaIds)
        {
            // 6. Obtener cada Subasta con sus Pujas
            var subasta = await _subastaRepository.ObtenerConPujasAsync(subastaId);
            if (subasta == null) continue;

            // 7. Calcular SigueAbierta
            var ahora = DateTime.UtcNow;
            bool sigueAbierta =
                subasta.Estado == EstadoSubasta.ACTIVA &&
                ahora >= subasta.FechaInicio &&
                ahora <= subasta.FechaFin;

            // 8. Determinar Gano
            bool? gano = null;

            if (subasta.Estado == EstadoSubasta.FINALIZADA)
            {
                if (subasta.Pujas != null && subasta.Pujas.Any())
                {
                    var mayorPuja = subasta.Pujas
                        .OrderByDescending(p => p.Monto)
                        .First();

                    gano = mayorPuja.CompradorId == query.CompradorId;
                }
                else
                {
                    gano = false;
                }
            }
            else if (subasta.Estado == EstadoSubasta.DESIERTA)
            {
                gano = false;
            }

            // 9. Agregar el DTO
            resultado.Add(new ActividadCompradorDto
            {
                SubastaId = subasta.Id,
                Titulo = subasta.Titulo,
                SigueAbierta = sigueAbierta,
                Gano = gano
            });
        }

        return resultado;
    }
}
