using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.Pujar;

public class CrearPujaHandler
{
    private readonly IRepository<Subasta> _subastaRepository;
    private readonly IRepository<Puja> _pujaRepository;
    private readonly IBilleteraRepository _billeteraRepository;
    private readonly ITransaccionRepository _transaccionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPujaHandler(
        IRepository<Subasta> subastaRepository,
        IRepository<Puja> pujaRepository,
        IBilleteraRepository billeteraRepository,
        ITransaccionRepository transaccionRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _pujaRepository = pujaRepository;
        _billeteraRepository = billeteraRepository;
        _transaccionRepository = transaccionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CrearPujaCommand command)
    {
        // Iniciar transacción antes del try
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Buscar la subasta
            var subasta = await _subastaRepository.ObtenerAsync(command.SubastaId);

            if (subasta == null)
                throw new NotFoundException("La subasta no existe.");

            // Validar estado de la subasta
            if (subasta.Estado != EstadoSubasta.ACTIVA)
                throw new DomainException("La subasta no está activa.");

            // Validar fechas de la subasta
            var ahora = DateTime.UtcNow;

            if (ahora < subasta.FechaInicio)
                throw new DomainException("La subasta todavía no comenzó.");

            if (ahora > subasta.FechaFin)
                throw new DomainException("La subasta ya finalizó.");

            // Buscar las pujas de esta subasta
            var todasLasPujas = await _pujaRepository.ListarAsync();

            var pujaActual = todasLasPujas
                .Where(p => p.SubastaId == command.SubastaId)
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            // Calcular el monto mínimo permitido
            decimal montoMinimo;

            if (pujaActual == null)
            {
                montoMinimo = subasta.PrecioBase;
            }
            else
            {
                montoMinimo = pujaActual.Monto + subasta.IncrementoMinimo;
            }

            if (command.Monto < montoMinimo)
            {
                throw new DomainException(
                    $"La puja mínima permitida es {montoMinimo}.");
            }

            // Buscar la billetera del comprador usando el repositorio específico
            var billeteraComprador = await _billeteraRepository.ObtenerPorUsuarioAsync(command.CompradorId);

            if (billeteraComprador == null)
                throw new DomainException("El comprador no tiene una billetera.");

            // Si el comprador ya era el mayor postor,
            // solamente debe retener la diferencia.
            if (pujaActual != null && pujaActual.CompradorId == command.CompradorId)
            {
                // Mismo comprador sigue siendo líder: retener sólo la diferencia
                var diferencia = command.Monto - pujaActual.Monto;

                if (billeteraComprador.SaldoDisponible < diferencia)
                    throw new DomainException("Saldo insuficiente.");

                billeteraComprador.Retener(diferencia);

                var tx = new TransaccionLedger(
                    billeteraComprador.Id,
                    TipoMovimiento.RETENCION,
                    diferencia,
                    ahora,
                    command.SubastaId);

                await _transaccionRepository.AgregarAsync(tx);
            }
            else
            {
                // Nuevo líder: retener el monto completo
                if (billeteraComprador.SaldoDisponible < command.Monto)
                    throw new DomainException("Saldo insuficiente.");

                billeteraComprador.Retener(command.Monto);

                var txNuevo = new TransaccionLedger(
                    billeteraComprador.Id,
                    TipoMovimiento.RETENCION,
                    command.Monto,
                    ahora,
                    command.SubastaId);

                await _transaccionRepository.AgregarAsync(txNuevo);

                // Si hay un anterior líder distinto, liberarlo
                if (pujaActual != null)
                {
                    var billeteraAnterior = await _billeteraRepository.ObtenerPorUsuarioAsync(pujaActual.CompradorId);

                    if (billeteraAnterior == null)
                        throw new DomainException("No se encontró la billetera del anterior comprador.");

                    billeteraAnterior.Liberar(pujaActual.Monto);

                    var txLiberacion = new TransaccionLedger(
                        billeteraAnterior.Id,
                        TipoMovimiento.LIBERACION,
                        pujaActual.Monto,
                        ahora,
                        command.SubastaId);

                    await _transaccionRepository.AgregarAsync(txLiberacion);

                    billeteraAnterior.IncrementarVersion();
                }
            }

            // Crear la nueva puja
            var nuevaPuja = new Puja(
                command.SubastaId,
                command.CompradorId,
                command.Monto,
                ahora);

            await _pujaRepository.AgregarAsync(nuevaPuja);

            // La subasta cambia con cada puja.
            // Esto permite detectar pujas concurrentes.
            subasta.IncrementarVersion();

            // Actualizar versión de la billetera del comprador
            billeteraComprador.IncrementarVersion();

            // Anti-sniping: extender si quedan 60 segundos o menos
            var tiempoRestante = subasta.FechaFin - ahora;
            if (tiempoRestante >= TimeSpan.Zero && tiempoRestante <= TimeSpan.FromSeconds(60))
            {
                subasta.ExtenderPorAntiSniping();
            }

            // Guardar todo
            await _unitOfWork.SaveChangesAsync();

            // Confirmar la transacción
            await _unitOfWork.CommitTransactionAsync();

            return nuevaPuja.Id;
        }
        catch
        {
            // Revertir la transacción ante cualquier error
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
