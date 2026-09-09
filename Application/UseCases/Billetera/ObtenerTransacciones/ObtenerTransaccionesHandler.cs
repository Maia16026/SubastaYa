using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.UseCases.Billetera.ObtenerTransacciones;

public class ObtenerTransaccionesHandler : IQueryHandler<ObtenerTransaccionesQuery, List<TransaccionDto>?>
{
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly ITransaccionRepository _transaccionRepository;

    public ObtenerTransaccionesHandler(
        IBilleteraRepository billeteraRepository,
        ITransaccionRepository transaccionRepository)
    {
        _billeteraRepository = billeteraRepository;
        _transaccionRepository = transaccionRepository;
    }

    public async Task<List<TransaccionDto>?> Handle(ObtenerTransaccionesQuery query, CancellationToken ct = default)
    {
        // Buscar billetera por UsuarioId
        var billetera = await _billeteraRepository.ObtenerPorUsuarioAsync(query.UsuarioId, ct);
        if (billetera is null)
            return null; // se mapeará a 404 en el controller

        // Obtener transacciones por BilleteraId
        var movimientos = await _transaccionRepository.ListarPorBilleteraAsync(billetera.Id, ct);

        // Mapear explícitamente a DTO
        var result = new List<TransaccionDto>(movimientos.Count);
        foreach (var m in movimientos)
        {
            result.Add(new TransaccionDto
            {
                Id = m.Id,
                Tipo = m.Tipo.ToString(),
                Monto = m.Monto,
                Fecha = m.Fecha,
                SubastaId = m.SubastaId
            });
        }

        return result;
    }
}
