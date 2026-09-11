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
    private readonly IRepository<Usuario> _usuarioRepository;
    private readonly IRepository<Categoria> _categoriaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CrearSubastaHandler(
        IRepository<Subasta> repository,
        IRepository<Usuario> usuarioRepository,
        IRepository<Categoria> categoriaRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _categoriaRepository = categoriaRepository;
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

        var fechaInicioUtc = command.FechaInicio.UtcDateTime;
        var fechaFinUtc = command.FechaFin.UtcDateTime;

        if (fechaFinUtc <= fechaInicioUtc)
        {
            throw new DomainException(
                "La fecha de finalización debe ser posterior a la fecha de inicio.");
        }

        var vendedor = await _usuarioRepository.ObtenerAsync(command.VendedorId);
        if (vendedor == null)
        {
            throw new DomainException("Vendedor inexistente");
        }

        var categoria = await _categoriaRepository.ObtenerAsync(command.CategoriaId);
        if (categoria == null)
        {
            throw new DomainException("Categoría inexistente");
        }

        var subasta = new Subasta(
            command.VendedorId,
            command.CategoriaId,
            command.Titulo,
            command.Descripcion,
            command.UrlImagen,
            command.PrecioBase,
            command.IncrementoMinimo,
            fechaInicioUtc,
            fechaFinUtc);

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