using System.Text.Json;
using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases.Subastas.ProcesarSubastas;

public class ProcesarSubastasHandler
{
    private readonly ISubastaRepository _subastaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly ITransaccionRepository _transaccionRepository;
    private readonly IRepository<AuditoriaLog> _auditoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcesarSubastasHandler(
        ISubastaRepository subastaRepository,
        IBilleteraRepository billeteraRepository,
        ITransaccionRepository transaccionRepository,
        IRepository<AuditoriaLog> auditoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _billeteraRepository = billeteraRepository;
        _transaccionRepository = transaccionRepository;
        _auditoriaRepository = auditoriaRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancellationToken ct = default)
    {
        var ahora = DateTime.UtcNow;

        var programadas = await _subastaRepository
            .ListarProgramadasParaActivarAsync(ahora, ct);

        foreach (var subasta in programadas)
        {
            await ActivarSubastaAsync(subasta, ahora, ct);
        }

        var vencidas = await _subastaRepository
            .ListarActivasVencidasConPujasAsync(ahora, ct);

        foreach (var subasta in vencidas)
        {
            await CerrarSubastaAsync(subasta, ahora, ct);
        }
    }

    private async Task ActivarSubastaAsync(
        Subasta subasta,
        DateTime ahora,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var estadoAnterior = subasta.Estado;

            subasta.Activar();
            subasta.IncrementarVersion();

            var detalleJson = JsonSerializer.Serialize(new
            {
                EstadoAnterior = estadoAnterior.ToString(),
                EstadoNuevo = subasta.Estado.ToString()
            });

            var auditoria = new AuditoriaLog(
                entidad: "SUBASTA",
                entidadId: subasta.Id,
                accion: "CAMBIO_ESTADO",
                detalleJson: detalleJson,
                fecha: ahora,
                usuarioId: null);

            await _auditoriaRepository.AgregarAsync(auditoria);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }

    private async Task CerrarSubastaAsync(
        Subasta subasta,
        DateTime ahora,
        CancellationToken ct)
    {
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            var estadoAnterior = subasta.Estado;

            var pujaGanadora = subasta.Pujas
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            if (pujaGanadora == null)
            {
                subasta.MarcarDesierta();
                subasta.IncrementarVersion();

                var detalleJson = JsonSerializer.Serialize(new
                {
                    EstadoAnterior = estadoAnterior.ToString(),
                    EstadoNuevo = subasta.Estado.ToString()
                });

                var auditoria = new AuditoriaLog(
                    entidad: "SUBASTA",
                    entidadId: subasta.Id,
                    accion: "CAMBIO_ESTADO",
                    detalleJson: detalleJson,
                    fecha: ahora,
                    usuarioId: null);

                await _auditoriaRepository.AgregarAsync(auditoria);

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                return;
            }

            var billeteraComprador =
                await _billeteraRepository.ObtenerPorUsuarioAsync(
                    pujaGanadora.CompradorId,
                    ct);

            if (billeteraComprador == null)
                throw new InvalidOperationException(
                    "No se encontró la billetera del comprador ganador.");

            var billeteraVendedor =
                await _billeteraRepository.ObtenerPorUsuarioAsync(
                    subasta.VendedorId,
                    ct);

            if (billeteraVendedor == null)
                throw new InvalidOperationException(
                    "No se encontró la billetera del vendedor.");

            billeteraComprador.Pagar(pujaGanadora.Monto);
            billeteraVendedor.Cobrar(pujaGanadora.Monto);

            var pago = new TransaccionLedger(
                billeteraComprador.Id,
                TipoMovimiento.PAGO,
                pujaGanadora.Monto,
                ahora,
                subasta.Id);

            var cobro = new TransaccionLedger(
                billeteraVendedor.Id,
                TipoMovimiento.COBRO,
                pujaGanadora.Monto,
                ahora,
                subasta.Id);

            await _transaccionRepository.AgregarAsync(pago);
            await _transaccionRepository.AgregarAsync(cobro);

            billeteraComprador.IncrementarVersion();
            billeteraVendedor.IncrementarVersion();

            subasta.Finalizar();
            subasta.IncrementarVersion();

            var detalleFinalizacion = JsonSerializer.Serialize(new
            {
                EstadoAnterior = estadoAnterior.ToString(),
                EstadoNuevo = subasta.Estado.ToString(),
                CompradorId = pujaGanadora.CompradorId,
                VendedorId = subasta.VendedorId,
                Monto = pujaGanadora.Monto
            });

            var auditoriaFinalizacion = new AuditoriaLog(
                entidad: "SUBASTA",
                entidadId: subasta.Id,
                accion: "CAMBIO_ESTADO",
                detalleJson: detalleFinalizacion,
                fecha: ahora,
                usuarioId: null);

            await _auditoriaRepository.AgregarAsync(auditoriaFinalizacion);

            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}