using Application.DTOs;
using Application.UseCases.Billetera.ObtenerBilletera;
using Application.UseCases.Billetera.DepositarSaldo;
using Application.UseCases.Billetera.ObtenerTransacciones;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/billeteras")]
public class BilleteraController : ControllerBase
{
    private readonly IQueryHandler<ObtenerBilleteraQuery, BilleteraDto?> _handler;
    private readonly ICommandHandler<DepositarSaldoCommand, BilleteraDto?> _depositHandler;
    private readonly IQueryHandler<ObtenerTransaccionesQuery, List<TransaccionDto>?> _transHandler;

    public BilleteraController(
        IQueryHandler<ObtenerBilleteraQuery, BilleteraDto?> handler,
        ICommandHandler<DepositarSaldoCommand, BilleteraDto?> depositHandler,
        IQueryHandler<ObtenerTransaccionesQuery, List<TransaccionDto>?> transHandler)
    {
        _handler = handler;
        _depositHandler = depositHandler;
        _transHandler = transHandler;
    }

    [HttpGet("{usuarioId}")]
    public async Task<IActionResult> ObtenerPorUsuario(int usuarioId)
    {
        var query = new ObtenerBilleteraQuery(usuarioId);
        var resultado = await _handler.Handle(query);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }

    // Modelo de petición para depósitos: el UsuarioId viene por la ruta, el cuerpo solo incluye el monto
    public record DepositoRequest(decimal Monto);

    [HttpPost("{usuarioId}/depositos")]
    public async Task<IActionResult> Depositar(int usuarioId, [FromBody] DepositoRequest request)
    {
        var command = new DepositarSaldoCommand(usuarioId, request.Monto);
        var resultado = await _depositHandler.Handle(command);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }

    [HttpGet("{usuarioId}/transacciones")]
    public async Task<IActionResult> ObtenerTransacciones(int usuarioId)
    {
        var query = new ObtenerTransaccionesQuery(usuarioId);
        var resultado = await _transHandler.Handle(query);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }
}
