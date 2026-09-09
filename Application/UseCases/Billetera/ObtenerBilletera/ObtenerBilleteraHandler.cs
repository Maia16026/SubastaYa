using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.UseCases.Billetera.ObtenerBilletera;

public class ObtenerBilleteraHandler : IQueryHandler<ObtenerBilleteraQuery, BilleteraDto?>
{
    private readonly IBilleteraRepository _repository;

    public ObtenerBilleteraHandler(IBilleteraRepository repository)
    {
        _repository = repository;
    }

    public async Task<BilleteraDto?> Handle(ObtenerBilleteraQuery query, CancellationToken ct = default)
    {
        var billetera = await _repository.ObtenerPorUsuarioAsync(query.UsuarioId, ct);

        if (billetera is null)
            return null;

        return new BilleteraDto
        {
            Id = billetera.Id,
            UsuarioId = billetera.UsuarioId,
            SaldoTotal = billetera.SaldoTotal,
            SaldoRetenido = billetera.SaldoRetenido,
            SaldoDisponible = billetera.SaldoDisponible
        };
    }
}
