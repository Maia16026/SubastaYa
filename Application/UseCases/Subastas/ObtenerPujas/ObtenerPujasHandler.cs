using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Persistence;
using Domain.Entities;


namespace Application.UseCases.Subastas.ObtenerPujas;

public class ObtenerPujasHandler
{
    private readonly IRepository<Puja> _repository;

    public ObtenerPujasHandler(IRepository<Puja> repository)
    {
        _repository = repository;
    }

    public async Task<List<PujaHistorialDto>> Handle(ObtenerPujasQuery query)
    {
        var pujas = await _repository.ListarAsync();

        var filtradas = pujas
            .Where(p => p.SubastaId == query.SubastaId)
            .OrderByDescending(p => p.FechaPuja)
            .ToList();

        var seudonimosPorComprador = new Dictionary<int,int>();
        var resultado = new List<PujaHistorialDto>();
        int contador = 0;

        foreach (var puja in filtradas)
        {
            if (!seudonimosPorComprador.TryGetValue(puja.CompradorId, out var numeroPostor))
            {
                contador++;
                numeroPostor = contador;
                seudonimosPorComprador[puja.CompradorId] = numeroPostor;
            }

            resultado.Add(new PujaHistorialDto
            {
                Seudonimo = $"Postor {numeroPostor}",
                Monto = puja.Monto,
                FechaHora = puja.FechaPuja
            });
        }

        return resultado;
    }
}
