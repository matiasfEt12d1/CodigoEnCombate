namespace Aplicacion.Dtos
{
    // Accion: "atacar" o "habilidad". El defensor es siempre el otro participante.
    public record EjecutarTurnoSolicitud(int AtacanteId, string? Accion);

    public record ActualizarNotaSolicitud(string? Nota);

    public record TurnoRespuesta(
        int Id,
        int Numero,
        int AtacanteId,
        int DefensorId,
        string Accion,
        int Danio,
        string? Nota,
        DateTime Fecha);

    public record TurnoEjecutadoRespuesta(TurnoRespuesta Turno, CombateEstadoRespuesta Combate);

    public record ErrorRespuesta(string Mensaje);
}
