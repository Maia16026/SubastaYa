namespace Application.UseCases.Billetera.DepositarSaldo;

public record DepositarSaldoCommand(int UsuarioId, decimal Monto);
