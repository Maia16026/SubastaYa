using Application.DTOs;
using Application.Interfaces.Persistence;
using Domain.Entities;

namespace Application.UseCases.Subastas.ObtenerPujasPorComprador;

public class ObtenerPujasPorCompradorHandler
{
    private readonly IRepository<Puja> _repository;

    public ObtenerPujasPorCompradorHandler(
        IRepository<Puja> repository)
    {
        _repository = repository;
    }

    public async Task<List<PujaDto>> Handle(
        ObtenerPujasPorCompradorQuery query)
    {
        var pujas = await _repository.ListarAsync();

        return pujas
            .Where(p => p.CompradorId == query.CompradorId)
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