using Aplicacion.Dtos;
using Aplicacion.Interfaces;
using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class CombateService : ICombateService
    {
        private readonly ICombateRepository _combates;
        private readonly IPersonajeApiRepository _personajes;
        private readonly ITurnoRepository _turnos;

        public CombateService(ICombateRepository combates, IPersonajeApiRepository personajes, ITurnoRepository turnos)
        {
            _combates = combates ?? throw new ArgumentNullException(nameof(combates));
            _personajes = personajes ?? throw new ArgumentNullException(nameof(personajes));
            _turnos = turnos ?? throw new ArgumentNullException(nameof(turnos));
        }

        public IReadOnlyList<CombateResumenRespuesta> ObtenerTodos()
        {
            var personajes = _personajes.ObtenerTodos().ToDictionary(p => p.Id);
            return _combates.ObtenerTodos()
                .Select(c => MapeoDtos.CombateResumen(c, personajes[c.Personaje1Id], personajes[c.Personaje2Id]))
                .ToList();
        }

        public Resultado<CombateDetalleRespuesta> ObtenerPorId(int id)
        {
            var combate = _combates.ObtenerPorId(id);
            if (combate == null) return Resultado<CombateDetalleRespuesta>.NoEncontrado($"No existe el combate {id}.");

            return Resultado<CombateDetalleRespuesta>.Ok(Detalle(combate, TipoConexion.ApiInvitado));
        }

        public Resultado<CombateDetalleRespuesta> Crear(ParticipantesSolicitud solicitud)
        {
            var error = ValidarParticipantes(solicitud);
            if (error != null) return Resultado<CombateDetalleRespuesta>.Invalido(error);

            var p1 = _personajes.ObtenerPorId(solicitud.Personaje1Id, TipoConexion.ApiAdministrador);
            var p2 = _personajes.ObtenerPorId(solicitud.Personaje2Id, TipoConexion.ApiAdministrador);
            var faltante = PersonajeFaltante(solicitud, p1, p2);
            if (faltante != null) return Resultado<CombateDetalleRespuesta>.NoEncontrado(faltante);

            var combate = CombateNuevo(p1!, p2!);
            _combates.Agregar(combate);

            return Resultado<CombateDetalleRespuesta>.Ok(Detalle(combate, TipoConexion.ApiAdministrador));
        }

        public Resultado<CombateDetalleRespuesta> ActualizarParticipantes(int id, ParticipantesSolicitud solicitud)
        {
            var error = ValidarParticipantes(solicitud);
            if (error != null) return Resultado<CombateDetalleRespuesta>.Invalido(error);

            var actual = _combates.ObtenerPorId(id, TipoConexion.ApiAdministrador);
            if (actual == null) return Resultado<CombateDetalleRespuesta>.NoEncontrado($"No existe el combate {id}.");

            var p1 = _personajes.ObtenerPorId(solicitud.Personaje1Id, TipoConexion.ApiAdministrador);
            var p2 = _personajes.ObtenerPorId(solicitud.Personaje2Id, TipoConexion.ApiAdministrador);
            var faltante = PersonajeFaltante(solicitud, p1, p2);
            if (faltante != null) return Resultado<CombateDetalleRespuesta>.NoEncontrado(faltante);

            var combate = CombateNuevo(p1!, p2!);
            combate.Id = id;
            combate.FechaCreacion = actual.FechaCreacion;

            if (!_combates.ActualizarParticipantesSiNoTieneTurnos(combate))
                return Resultado<CombateDetalleRespuesta>.Conflicto($"El combate {id} ya tiene turnos y no se pueden cambiar sus participantes.");

            return Resultado<CombateDetalleRespuesta>.Ok(Detalle(combate, TipoConexion.ApiAdministrador));
        }

        public Resultado Eliminar(int id)
        {
            if (_combates.ObtenerPorId(id, TipoConexion.ApiAdministrador) == null)
                return Resultado.NoEncontrado($"No existe el combate {id}.");

            return _combates.EliminarSiNoTieneTurnos(id)
                ? Resultado.Ok()
                : Resultado.Conflicto($"El combate {id} ya tiene turnos y no se puede eliminar.");
        }

        // Estado inicial: vida completa, recurso inicial de cada tipo y empieza el personaje 1.
        private static CombateRegistro CombateNuevo(PersonajeRegistro p1, PersonajeRegistro p2) => new()
        {
            Personaje1Id = p1.Id,
            Personaje2Id = p2.Id,
            Estado = EstadoCombate.EnCurso,
            TurnoPersonaje1 = true,
            NumeroTurno = 1,
            VidaPersonaje1 = p1.VidaMax,
            VidaPersonaje2 = p2.VidaMax,
            RecursoPersonaje1 = CatalogoPersonajes.RecursoInicial(p1),
            RecursoPersonaje2 = CatalogoPersonajes.RecursoInicial(p2),
            GanadorId = null
        };

        private static string? ValidarParticipantes(ParticipantesSolicitud? solicitud)
        {
            if (solicitud == null) return "Debe indicar los dos participantes.";
            if (solicitud.Personaje1Id <= 0 || solicitud.Personaje2Id <= 0) return "Los ID de los participantes deben ser mayores a cero.";
            if (solicitud.Personaje1Id == solicitud.Personaje2Id) return "Los participantes deben ser dos personajes distintos.";
            return null;
        }

        private static string? PersonajeFaltante(ParticipantesSolicitud solicitud, PersonajeRegistro? p1, PersonajeRegistro? p2)
        {
            if (p1 == null) return $"No existe el personaje {solicitud.Personaje1Id}.";
            if (p2 == null) return $"No existe el personaje {solicitud.Personaje2Id}.";
            return null;
        }

        private CombateDetalleRespuesta Detalle(CombateRegistro combate, TipoConexion conexion)
        {
            var p1 = _personajes.ObtenerPorId(combate.Personaje1Id, conexion)!;
            var p2 = _personajes.ObtenerPorId(combate.Personaje2Id, conexion)!;
            var turnos = combate.Id > 0 ? _turnos.ObtenerPorCombate(combate.Id, conexion) : [];
            return MapeoDtos.CombateDetalle(combate, p1, p2, turnos);
        }
    }
}
