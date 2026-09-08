using Application.DTOs;
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

    public async Task<List<PujaDto>> Handle(ObtenerPujasQuery query)
    {
        var pujas = await _repository.ListarAsync();

        return pujas
            .Where(p => p.SubastaId == query.SubastaId)
            .OrderByDescending(p => p.FechaPuja)
            .Select(p => new PujaDto
            {
                Id = p.Id,
                SubastaId = p.SubastaId,
                UsuarioId = p.CompradorId,
                Monto = p.Monto,
                FechaHora = p.FechaPuja
            })
            .ToList();
    }
}