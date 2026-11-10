using Aplicacion.Dtos;
using Persistencia.Entidades;

namespace Aplicacion.Servicios
{
    // Transforma registros de Persistencia en DTO de respuesta. Las entidades nunca salen de la capa de aplicación.
    internal static class MapeoDtos
    {
        public static PersonajeRespuesta Personaje(PersonajeRegistro p) => new(
            p.Id, p.Nombre, p.Tipo, p.VidaMax, p.Ataque, p.Defensa,
            p.Escudo, p.ManaMax, p.CantidadFlechas, p.ProbabilidadCritico);

        public static TurnoRespuesta Turno(TurnoRegistro t) => new(
            t.Id, t.Numero, t.AtacanteId, t.DefensorId, t.Accion, t.Danio, t.Nota, t.Fecha);

        public static CombateResumenRespuesta CombateResumen(CombateRegistro c, PersonajeRegistro p1, PersonajeRegistro p2) => new(
            c.Id,
            new ParticipanteResumen(p1.Id, p1.Nombre, p1.Tipo),
            new ParticipanteResumen(p2.Id, p2.Nombre, p2.Tipo),
            c.Estado,
            c.GanadorId);

        public static CombateEstadoRespuesta CombateEstado(CombateRegistro c, PersonajeRegistro p1, PersonajeRegistro p2) => new(
            c.Id,
            c.Estado,
            c.NumeroTurno,
            c.IdQueActua,
            new ParticipanteEstado(p1.Id, p1.Nombre, p1.Tipo, c.VidaPersonaje1, p1.VidaMax, c.RecursoPersonaje1),
            new ParticipanteEstado(p2.Id, p2.Nombre, p2.Tipo, c.VidaPersonaje2, p2.VidaMax, c.RecursoPersonaje2),
            c.GanadorId,
            NombreGanador(c, p1, p2));

        public static CombateDetalleRespuesta CombateDetalle(CombateRegistro c, PersonajeRegistro p1, PersonajeRegistro p2, IEnumerable<TurnoRegistro> turnos)
        {
            var estado = CombateEstado(c, p1, p2);
            return new CombateDetalleRespuesta(
                estado.Id, estado.Estado, estado.NumeroTurno, estado.TurnoDeId,
                estado.Personaje1, estado.Personaje2, estado.GanadorId, estado.GanadorNombre,
                turnos.Select(Turno).ToList());
        }

        private static string? NombreGanador(CombateRegistro c, PersonajeRegistro p1, PersonajeRegistro p2) =>
            c.GanadorId == p1.Id ? p1.Nombre : c.GanadorId == p2.Id ? p2.Nombre : null;
    }
}
