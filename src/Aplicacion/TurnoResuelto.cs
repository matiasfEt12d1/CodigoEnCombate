using Persistencia.Entidades;

namespace Aplicacion
{
    // Una acción ya ejecutada. HabilidadId es null cuando fue un ataque básico.
    // Danio incluye lo que absorbió el defensor antes de perder vida (por ejemplo, el escudo).
    public record AccionResuelta(
        Personaje Atacante,
        Personaje Defensor,
        AccionCombate Accion,
        int? HabilidadId,
        int Danio);

    // Resultado de un turno: la acción de cada combatiente que llegó a actuar y el estado final.
    public record TurnoResuelto(
        int NumeroTurno,
        IReadOnlyList<AccionResuelta> Acciones,
        bool Finalizada,
        Personaje? Ganador);
}
