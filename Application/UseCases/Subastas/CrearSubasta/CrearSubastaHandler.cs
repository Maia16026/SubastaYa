using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.UseCases.Subastas.CrearSubasta;

public class CrearSubastaHandler
    : ICommandHandler<CrearSubastaCommand, SubastaDto>
{
    private readonly IRepository<Subasta> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearSubastaHandler(
        IRepository<Subasta> repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubastaDto> Handle(
        CrearSubastaCommand command,
        CancellationToken ct = default)
    {
        var subasta = new Subasta(
            command.VendedorId,
            command.Titulo,
            command.PrecioBase,
            command.IncrementoMinimo,
            command.FechaInicio,
            command.FechaFin);

        await _repository.AgregarAsync(subasta);

        await _unitOfWork.SaveChangesAsync(ct);

        return new SubastaDto
        {
            Id = subasta.Id,
            VendedorId = subasta.VendedorId,
            Titulo = subasta.Titulo,
            PrecioBase = subasta.PrecioBase,
            IncrementoMinimo = subasta.IncrementoMinimo,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Estado = subasta.Estado.ToString(),
            GanadorId = subasta.GanadorId
        };
    }
}