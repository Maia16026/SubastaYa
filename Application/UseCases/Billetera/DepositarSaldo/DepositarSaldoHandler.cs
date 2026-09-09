using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Enums;

namespace Application.UseCases.Billetera.DepositarSaldo;

public class DepositarSaldoHandler : ICommandHandler<DepositarSaldoCommand, BilleteraDto?>
{
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly IRepository<TransaccionLedger> _transRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositarSaldoHandler(
        IBilleteraRepository billeteraRepository,
        IRepository<TransaccionLedger> transRepository,
        IUnitOfWork unitOfWork)
    {
        _billeteraRepository = billeteraRepository;
        _transRepository = transRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BilleteraDto?> Handle(DepositarSaldoCommand command, CancellationToken ct = default)
    {
        if (command.Monto <= 0)
            throw new DomainException("El monto debe ser mayor que 0.");

        var billetera = await _billeteraRepository.ObtenerPorUsuarioAsync(command.UsuarioId, ct);
        if (billetera is null)
            return null;

        // Aplicar lógica de dominio
        billetera.Depositar(command.Monto);

        // Crear movimiento ledger
        var tx = new TransaccionLedger(billetera.Id, TipoMovimiento.DEPOSITO, command.Monto, DateTime.UtcNow, null);
        await _transRepository.AgregarAsync(tx);

        // La entidad Billetera fue obtenida por el mismo DbContext y está siendo
        // rastreada por EF Core. No es necesario llamar a Update; SaveChangesAsync
        // detectará y persistirá los cambios en SaldoTotal/SaldoDisponible.
        await _unitOfWork.SaveChangesAsync(ct);

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
