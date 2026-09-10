using System.Text.Json;
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
    private readonly IRepository<AuditoriaLog> _auditoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositarSaldoHandler(
        IBilleteraRepository billeteraRepository,
        IRepository<TransaccionLedger> transRepository,
        IRepository<AuditoriaLog> auditoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _billeteraRepository = billeteraRepository;
        _transRepository = transRepository;
        _auditoriaRepository = auditoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BilleteraDto?> Handle(
        DepositarSaldoCommand command,
        CancellationToken ct = default)
    {
        if (command.Monto <= 0)
            throw new DomainException("El monto debe ser mayor que 0.");

        var billetera = await _billeteraRepository.ObtenerPorUsuarioAsync(
            command.UsuarioId,
            ct);

        if (billetera is null)
            return null;

        billetera.Depositar(command.Monto);

        var tx = new TransaccionLedger(
            billetera.Id,
            TipoMovimiento.DEPOSITO,
            command.Monto,
            DateTime.UtcNow,
            null);

        await _transRepository.AgregarAsync(tx);

        var detalleJson = JsonSerializer.Serialize(new
        {
            Monto = command.Monto,
            SaldoTotal = billetera.SaldoTotal,
            SaldoDisponible = billetera.SaldoDisponible
        });

        var auditoria = new AuditoriaLog(
            entidad: "BILLETERA",
            entidadId: billetera.Id,
            accion: "ACREDITACION_MANUAL",
            detalleJson: detalleJson,
            fecha: DateTime.UtcNow,
            usuarioId: command.UsuarioId);

        await _auditoriaRepository.AgregarAsync(auditoria);

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