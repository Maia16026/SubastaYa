using Application.DTOs;
using Application.Interfaces.Services;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerSubastaPorId;
using Application.UseCases.Subastas.ObtenerSubastas;
using Microsoft.AspNetCore.Mvc;
using Application.UseCases.Subastas.ObtenerSubastasPorVendedor;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubastasController : ControllerBase
{
    private readonly ICommandHandler<CrearSubastaCommand, SubastaDto> _crearSubastaHandler;

    private readonly IQueryHandler<
        ObtenerSubastasQuery,
        List<SubastaDto>> _obtenerSubastasHandler;

    private readonly IQueryHandler<
        ObtenerSubastaPorIdQuery,
        SubastaDto?> _obtenerSubastaPorIdHandler;

    public SubastasController(
        ICommandHandler<CrearSubastaCommand, SubastaDto> crearSubastaHandler,
        IQueryHandler<ObtenerSubastasQuery, List<SubastaDto>> obtenerSubastasHandler,
        IQueryHandler<ObtenerSubastaPorIdQuery, SubastaDto?> obtenerSubastaPorIdHandler)
    {
        _crearSubastaHandler = crearSubastaHandler;
        _obtenerSubastasHandler = obtenerSubastasHandler;
        _obtenerSubastaPorIdHandler = obtenerSubastaPorIdHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearSubastaCommand command)
    {
        var resultado = await _crearSubastaHandler.Handle(command);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = resultado.Id },
            resultado);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        [FromQuery] Domain.Enums.EstadoSubasta? estado,
        [FromQuery] int? categoriaId,
        [FromQuery] decimal? precioMin,
        [FromQuery] decimal? precioMax,
        [FromQuery] string? orden)
    {
        var query = new ObtenerSubastasQuery(
            estado,
            categoriaId,
            precioMin,
            precioMax,
            orden);

        var resultado = await _obtenerSubastasHandler.Handle(query);

        return Ok(resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var query = new ObtenerSubastaPorIdQuery(id);

        var resultado = await _obtenerSubastaPorIdHandler.Handle(query);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpGet("/api/vendedores/{vendedorId}/subastas")]
    public async Task<IActionResult> ObtenerPorVendedor(int vendedorId, [FromServices] ObtenerSubastasPorVendedorHandler handler)
    {
        var query = new ObtenerSubastasPorVendedorQuery(vendedorId);
        var resultado = await handler.Handle(query);

        return Ok(resultado);
    }
}