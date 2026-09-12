using Application.DTOs;
using Application.UseCases.Subastas.Pujar;
using Application.UseCases.Subastas.ObtenerPujas;
using Application.UseCases.Subastas.ObtenerPujasPorComprador; 
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/subastas/{subastaId}/pujas")]
public class PujasController : ControllerBase
{
    private readonly CrearPujaHandler _crearPujaHandler;
    private readonly ObtenerPujasHandler _obtenerPujasHandler;
    private readonly ObtenerPujasPorCompradorHandler _obtenerPujasPorCompradorHandler; 

    public PujasController(
        CrearPujaHandler crearPujaHandler,
        ObtenerPujasHandler obtenerPujasHandler,
        ObtenerPujasPorCompradorHandler obtenerPujasPorCompradorHandler) 
    {
        _crearPujaHandler = crearPujaHandler;
        _obtenerPujasHandler = obtenerPujasHandler;
        _obtenerPujasPorCompradorHandler = obtenerPujasPorCompradorHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        int subastaId,
        [FromBody] PujaDto dto)
    {
        var command = new CrearPujaCommand(
            subastaId,
            dto.UsuarioId,
            dto.Monto);

        var pujaId = await _crearPujaHandler.Handle(command);

        return Created(
            $"/api/subastas/{subastaId}/pujas/{pujaId}",
            new { id = pujaId });
    }

    [HttpGet]
    public async Task<IActionResult> Obtener(
        int subastaId)
    {
        var resultado = await _obtenerPujasHandler.Handle(
            new ObtenerPujasQuery(subastaId));

        return Ok(resultado);
    }

   
    [HttpGet("/api/compradores/{compradorId}/pujas")]
    public async Task<IActionResult> ObtenerPorComprador(int compradorId)
    {
        var query = new ObtenerPujasPorCompradorQuery(compradorId);
        
        var resultado = await _obtenerPujasPorCompradorHandler.Handle(query);

        return Ok(resultado);
    }
}