using Domain.Entities;

namespace Application.Interfaces.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(
        CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(
        CancellationToken cancellationToken = default);

    Task RegistrarEventoIndependienteAsync(
        AuditoriaLog log,
        CancellationToken cancellationToken = default);
}