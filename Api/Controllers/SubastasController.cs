using Application.DTOs;
using Application.Interfaces.Services;
using Application.UseCases.Subastas.CrearSubasta;
using Application.UseCases.Subastas.ObtenerSubastas;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")] 
public class SubastasController : ControllerBase
{
    private readonly ICommandHandler<CrearSubastaCommand, SubastaDto> _crearSubastaHandler;
    private readonly IQueryHandler<ObtenerSubastasQuery, SubastaDto?> _obtenerSubastasHandler;

    public SubastasController(
        ICommandHandler<CrearSubastaCommand, SubastaDto> crearSubastaHandler,
        IQueryHandler<ObtenerSubastasQuery, SubastaDto?> obtenerSubastasHandler)
    {
        _crearSubastaHandler = crearSubastaHandler;
        _obtenerSubastasHandler = obtenerSubastasHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSubastaCommand command)
    {
        var resultado = await _crearSubastaHandler.Handle(command);
        
        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id }, resultado);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var query = new ObtenerSubastasQuery(id);
        
        var resultado = await _obtenerSubastasHandler.Handle(query);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }
}