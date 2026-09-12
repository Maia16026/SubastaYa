using Application.Interfaces.Persistence;
using Domain.Enums;

namespace Application.UseCases.Subastas.ObtenerSubastasPorVendedor;

public class ObtenerSubastasPorVendedorHandler
{
    private readonly ISubastaRepository _subastaRepository;

    public ObtenerSubastasPorVendedorHandler(
        ISubastaRepository subastaRepository)
    {
        _subastaRepository = subastaRepository;
    }

    public async Task<List<PublicacionVendedorDto>> Handle(
        ObtenerSubastasPorVendedorQuery query)
    {
        // 1. Obtener subastas con sus pujas
        var subastas = await _subastaRepository.ListarConPujasAsync(
            estado: null,
            categoriaId: null,
            precioMin: null,
            precioMax: null,
            orden: null);

        // 2. Filtrar por VendedorId y 3. ordenar
        var subastasDelVendedor = subastas
            .Where(s => s.VendedorId == query.VendedorId)
            .OrderByDescending(s => s.FechaInicio)
            .ToList();

        // 4. Crear lista resultado
        var resultado = new List<PublicacionVendedorDto>();

        // 5. foreach
        foreach (var subasta in subastasDelVendedor)
        {
            // 6. CantidadPujas
            var cantidadPujas = subasta.Pujas.Count;

            // 7. PujaActual
            decimal? pujaActual = null;
            if (subasta.Pujas.Any())
            {
                pujaActual = subasta.Pujas.Max(p => p.Monto);
            }

            // 8. MontoAdjudicado: solo para FINALIZADA con pujas
            decimal? montoAdjudicado = null;
            if (subasta.Estado == EstadoSubasta.FINALIZADA && cantidadPujas > 0)
            {
                montoAdjudicado = pujaActual;
            }

            // 9. Agregar DTO
            resultado.Add(new PublicacionVendedorDto
            {
                SubastaId = subasta.Id,
                Titulo = subasta.Titulo,
                Estado = subasta.Estado.ToString(),
                PujaActual = pujaActual,
                CantidadPujas = cantidadPujas,
                MontoAdjudicado = montoAdjudicado
            });
        }

        // 10. return
        return resultado;
    }
}
