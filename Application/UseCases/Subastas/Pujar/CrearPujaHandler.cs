using Application.Interfaces.Persistence;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Application.UseCases.Subastas.Pujar;

public class CrearPujaHandler
{
    private readonly IRepository<Subasta> _subastaRepository;
    private readonly IRepository<Puja> _pujaRepository;
    private readonly IRepository<Billetera> _billeteraRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearPujaHandler(
        IRepository<Subasta> subastaRepository,
        IRepository<Puja> pujaRepository,
        IRepository<Billetera> billeteraRepository,
        IUnitOfWork unitOfWork)
    {
        _subastaRepository = subastaRepository;
        _pujaRepository = pujaRepository;
        _billeteraRepository = billeteraRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CrearPujaCommand command)
    {
        // El inicio de la transacción va AFUERA del try, como te corrigieron
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Buscar la subasta
            var subasta = await _subastaRepository.ObtenerAsync(command.SubastaId);

            if (subasta == null)
                throw new DomainException("La subasta no existe.");

            // 2. Validar estado
            if (subasta.Estado != EstadoSubasta.ACTIVA)
                throw new DomainException("La subasta no está activa.");

            // 3. Validar fechas
            var ahora = DateTime.UtcNow;

            if (ahora < subasta.FechaInicio)
                throw new DomainException("La subasta todavía no comenzó.");

            if (ahora > subasta.FechaFin)
                throw new DomainException("La subasta ya finalizó.");

            // 4. Validar que el comprador no sea el vendedor
            if (subasta.VendedorId == command.CompradorId)
                throw new DomainException("El vendedor no puede pujar en su propia subasta.");

            // 5. Buscar las pujas de esta subasta
            var todasLasPujas = await _pujaRepository.ListarAsync();

            var pujaActual = todasLasPujas
                .Where(p => p.SubastaId == command.SubastaId)
                .OrderByDescending(p => p.Monto)
                .FirstOrDefault();

            // 6. Calcular el monto mínimo permitido
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

            // 7. Buscar la billetera del comprador
            var billeteras = await _billeteraRepository.ListarAsync();

            var billeteraComprador = billeteras
                .FirstOrDefault(b => b.UsuarioId == command.CompradorId);

            if (billeteraComprador == null)
                throw new DomainException("El comprador no tiene una billetera.");

            // 8. Si el comprador ya era el mayor postor,
            // solamente debe retener la diferencia.
            if (pujaActual != null &&
                pujaActual.CompradorId == command.CompradorId)
            {
                var diferencia = command.Monto - pujaActual.Monto;

                if (billeteraComprador.SaldoDisponible < diferencia)
                    throw new DomainException("Saldo insuficiente.");

                billeteraComprador.Retener(diferencia);
            }
            else
            {
                // 9. Si es un nuevo comprador, debe retener
                // el monto completo de la nueva puja.
                if (billeteraComprador.SaldoDisponible < command.Monto)
                    throw new DomainException("Saldo insuficiente.");

                billeteraComprador.Retener(command.Monto);

                // 10. Liberar el dinero retenido del anterior
                // mayor postor.
                if (pujaActual != null)
                {
                    var billeteraAnterior = billeteras
                        .FirstOrDefault(b =>
                            b.UsuarioId == pujaActual.CompradorId);

                    if (billeteraAnterior == null)
                        throw new DomainException(
                            "No se encontró la billetera del anterior comprador.");

                    billeteraAnterior.Liberar(pujaActual.Monto);
                    billeteraAnterior.IncrementarVersion();
                }
            }

            // 11. Crear la nueva puja
            var nuevaPuja = new Puja(
                command.SubastaId,
                command.CompradorId,
                command.Monto,
                ahora);

            await _pujaRepository.AgregarAsync(nuevaPuja);

            // La subasta cambia con cada puja.
            // Esto permite detectar pujas concurrentes.
            subasta.IncrementarVersion();

            // 12. Actualizar versión de la billetera del comprador
            billeteraComprador.IncrementarVersion();

            // 13. Anti-sniping:
            // Aplicamos la corrección sugerida para que no detecte subastas vencidas
            var tiempoRestante = subasta.FechaFin - ahora;
            if (tiempoRestante >= TimeSpan.Zero && tiempoRestante <= TimeSpan.FromSeconds(60))
            {
                subasta.ExtenderPorAntiSniping();
            }

            // 14. Guardar todo
            await _unitOfWork.SaveChangesAsync();

            // 15. Confirmar la transacción
            await _unitOfWork.CommitTransactionAsync();

            return nuevaPuja.Id;
        }
        catch
        {
            // 16. Rollback limpio y throw para que el ExceptionMiddleware decida (400 o 500)
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}