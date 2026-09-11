using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface ITransaccionRepository : IRepository<TransaccionLedger>
{
    Task<List<TransaccionLedger>> ListarPorBilleteraAsync(int billeteraId, CancellationToken ct = default);
}
