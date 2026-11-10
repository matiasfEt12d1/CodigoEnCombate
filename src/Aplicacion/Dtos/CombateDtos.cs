namespace Aplicacion.Dtos
{
    // Usado por POST y PUT de combates.
    public record ParticipantesSolicitud(int Personaje1Id, int Personaje2Id);

    public record ParticipanteResumen(int Id, string Nombre, string Tipo);

    public record ParticipanteEstado(int Id, string Nombre, string Tipo, int Vida, int VidaMax, int? Recurso);

    // Elemento de las listas de combates.
    public record CombateResumenRespuesta(
        int Id,
        ParticipanteResumen Personaje1,
        ParticipanteResumen Personaje2,
        string Estado,
        int? GanadorId);

    // Estado actual del combate. TurnoDeId es quien actúa a continuación (null si finalizó).
    public record CombateEstadoRespuesta(
        int Id,
        string Estado,
        int NumeroTurno,
        int? TurnoDeId,
        ParticipanteEstado Personaje1,
        ParticipanteEstado Personaje2,
        int? GanadorId,
        string? GanadorNombre);

    public record CombateDetalleRespuesta(
        int Id,
        string Estado,
        int NumeroTurno,
        int? TurnoDeId,
        ParticipanteEstado Personaje1,
        ParticipanteEstado Personaje2,
        int? GanadorId,
        string? GanadorNombre,
        IReadOnlyList<TurnoRespuesta> Turnos);
}
