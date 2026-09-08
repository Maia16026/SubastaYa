using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Exceptions;

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
        if (command.PrecioBase <= 0)
        {
            throw new DomainException(
                "El precio base debe ser mayor a cero.");
        }

        if (command.IncrementoMinimo <= 0)
        {
            throw new DomainException(
                "El incremento mínimo debe ser mayor a cero.");
        }

        if (command.FechaFin <= command.FechaInicio)
        {
            throw new DomainException(
                "La fecha de finalización debe ser posterior a la fecha de inicio.");
        }

        var subasta = new Subasta(
            command.VendedorId,
            command.CategoriaId,
            command.Titulo,
            command.Descripcion,
            command.UrlImagen,
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
            CategoriaId = subasta.CategoriaId,
            Titulo = subasta.Titulo,
            Descripcion = subasta.Descripcion,
            UrlImagen = subasta.UrlImagen,
            PrecioBase = subasta.PrecioBase,
            IncrementoMinimo = subasta.IncrementoMinimo,
            FechaInicio = subasta.FechaInicio,
            FechaFin = subasta.FechaFin,
            Estado = subasta.Estado.ToString(),
            PujaActual = null,
            CantidadPujas = 0
        };
    }
}