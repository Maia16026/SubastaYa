using System;
namespace Domain.Entities;

public class AuditoriaLog
{
    public int Id { get; private set; }

    public string Entidad { get; private set; } = null!;

    public int EntidadId { get; private set; }

    public string Accion { get; private set; } = null!;

    public int? UsuarioId { get; private set; }

    public string DetalleJson { get; private set; } = null!;

    public DateTime Fecha { get; private set; }

    // Navegación
    public Usuario? Usuario { get; private set; }

    protected AuditoriaLog() { }

    public AuditoriaLog(string entidad, int entidadId, string accion, string detalleJson, DateTime fecha, int? usuarioId = null)
    {
        Entidad = entidad;
        EntidadId = entidadId;
        Accion = accion;
        DetalleJson = detalleJson;
        Fecha = fecha;
        UsuarioId = usuarioId;
    }
}
