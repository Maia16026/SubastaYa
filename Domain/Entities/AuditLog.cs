using System;
using Domain.Exceptions;

namespace Domain.Entities;

public class AuditLog
{
    public long Id { get; private set; }
    public string Evento { get; private set; }
    public string? Detalles { get; private set; }
    public int? UsuarioId { get; private set; }
    public int? SubastaId { get; private set; }
    public DateTime FechaHora { get; private set; }

    protected AuditLog() { }

    public AuditLog(string evento, string? detalles, DateTime fechaHora, int? usuarioId = null, int? subastaId = null)
    {
        if (string.IsNullOrWhiteSpace(evento)) throw new DomainException("El evento es obligatorio.");

        Evento = evento;
        Detalles = detalles;
        FechaHora = fechaHora;
        UsuarioId = usuarioId;
        SubastaId = subastaId;
    }
}