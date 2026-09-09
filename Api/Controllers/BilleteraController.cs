using Application.DTOs;
using Application.UseCases.Billetera.ObtenerBilletera;
using Application.UseCases.Billetera.DepositarSaldo;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BilleteraController : ControllerBase
{
    private readonly IQueryHandler<ObtenerBilleteraQuery, BilleteraDto?> _handler;
    private readonly ICommandHandler<DepositarSaldoCommand, BilleteraDto?> _depositHandler;

    public BilleteraController(
        IQueryHandler<ObtenerBilleteraQuery, BilleteraDto?> handler,
        ICommandHandler<DepositarSaldoCommand, BilleteraDto?> depositHandler)
    {
        _handler = handler;
        _depositHandler = depositHandler;
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

    [HttpPost("depositar")]
    public async Task<IActionResult> Depositar([FromBody] DepositarSaldoCommand command)
    {
        var resultado = await _depositHandler.Handle(command);

        if (resultado is null)
            return NotFound();

        return Ok(resultado);
    }
}
