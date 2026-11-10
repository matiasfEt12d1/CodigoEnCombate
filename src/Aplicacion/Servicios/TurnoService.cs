using Aplicacion.Dtos;
using Aplicacion.Interfaces;
using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class TurnoService : ITurnoService
    {
        public const int LargoMaximoNota = 255;

        // Cada acción delega en la Batalla del dominio; el comportamiento lo resuelve cada personaje (polimorfismo).
        private static readonly Dictionary<string, Action<Batalla, Personaje>> Acciones = new(StringComparer.OrdinalIgnoreCase)
        {
            ["atacar"] = (batalla, atacante) => batalla.EjecutarAtaque(atacante),
            ["habilidad"] = (batalla, atacante) => batalla.EjecutarHabilidad(atacante)
        };

        private readonly ITurnoRepository _turnos;
        private readonly ICombateRepository _combates;
        private readonly IPersonajeApiRepository _personajes;

        public TurnoService(ITurnoRepository turnos, ICombateRepository combates, IPersonajeApiRepository personajes)
        {
            _turnos = turnos ?? throw new ArgumentNullException(nameof(turnos));
            _combates = combates ?? throw new ArgumentNullException(nameof(combates));
            _personajes = personajes ?? throw new ArgumentNullException(nameof(personajes));
        }

        public Resultado<IReadOnlyList<TurnoRespuesta>> ObtenerPorCombate(int combateId)
        {
            if (_combates.ObtenerPorId(combateId) == null)
                return Resultado<IReadOnlyList<TurnoRespuesta>>.NoEncontrado($"No existe el combate {combateId}.");

            IReadOnlyList<TurnoRespuesta> turnos = _turnos.ObtenerPorCombate(combateId).Select(MapeoDtos.Turno).ToList();
            return Resultado<IReadOnlyList<TurnoRespuesta>>.Ok(turnos);
        }

        public Resultado<TurnoEjecutadoRespuesta> Ejecutar(int combateId, EjecutarTurnoSolicitud solicitud)
        {
            var combate = _combates.ObtenerPorId(combateId, TipoConexion.Administrador);
            if (combate == null)
                return Resultado<TurnoEjecutadoRespuesta>.NoEncontrado($"No existe el combate {combateId}.");

            var accionPedida = solicitud?.Accion?.Trim() ?? string.Empty;
            if (!Acciones.TryGetValue(accionPedida, out var ejecutarAccion))
                return Resultado<TurnoEjecutadoRespuesta>.Invalido("La acción debe ser 'atacar' o 'habilidad'.");

            int atacanteId = solicitud!.AtacanteId;
            if (!combate.Participa(atacanteId))
                return Resultado<TurnoEjecutadoRespuesta>.NoEncontrado($"El atacante {atacanteId} no participa en el combate {combateId}.");

            if (combate.EstaFinalizado)
                return Resultado<TurnoEjecutadoRespuesta>.Invalido("El combate ya finalizó.");

            if (combate.IdQueActua != atacanteId)
                return Resultado<TurnoEjecutadoRespuesta>.Invalido($"No es el turno del personaje {atacanteId}.");

            var registro1 = _personajes.ObtenerPorId(combate.Personaje1Id, TipoConexion.Administrador)!;
            var registro2 = _personajes.ObtenerPorId(combate.Personaje2Id, TipoConexion.Administrador)!;
            var p1 = CatalogoPersonajes.CrearEnCombate(registro1, combate.VidaPersonaje1, combate.RecursoPersonaje1);
            var p2 = CatalogoPersonajes.CrearEnCombate(registro2, combate.VidaPersonaje2, combate.RecursoPersonaje2);

            var batalla = new Batalla(p1, p2)
            {
                Id = combate.Id,
                NumeroTurno = combate.NumeroTurno,
                EsTurnoDeCombatiente1 = combate.TurnoPersonaje1
            };

            var atacante = batalla.AtacanteActual;
            var defensor = batalla.DefensorActual;
            var registroDefensor = ReferenceEquals(defensor, p1) ? registro1 : registro2;
            int vidaDefensorAntes = defensor.Vida;
            int recursoDefensorAntes = CatalogoPersonajes.LeerRecurso(registroDefensor, defensor) ?? 0;

            try
            {
                ejecutarAccion(batalla, atacante);
            }
            catch (InvalidOperationException ex)
            {
                return Resultado<TurnoEjecutadoRespuesta>.Invalido(ex.Message);
            }

            // Daño = vida perdida + recurso perdido al defender (por ejemplo, el escudo del Guerrero).
            int recursoDefensorDespues = CatalogoPersonajes.LeerRecurso(registroDefensor, defensor) ?? 0;
            int danio = (vidaDefensorAntes - defensor.Vida) + Math.Max(0, recursoDefensorAntes - recursoDefensorDespues);

            var turno = new TurnoRegistro
            {
                CombateId = combate.Id,
                Numero = combate.NumeroTurno,
                AtacanteId = atacante.Id,
                DefensorId = defensor.Id,
                Accion = accionPedida.ToLowerInvariant(),
                Danio = danio,
                Nota = null,
                VidaPersonaje1Antes = combate.VidaPersonaje1,
                VidaPersonaje2Antes = combate.VidaPersonaje2,
                RecursoPersonaje1Antes = combate.RecursoPersonaje1,
                RecursoPersonaje2Antes = combate.RecursoPersonaje2,
                Fecha = DateTime.Now
            };

            var actualizado = new CombateRegistro
            {
                Id = combate.Id,
                Personaje1Id = combate.Personaje1Id,
                Personaje2Id = combate.Personaje2Id,
                Estado = batalla.EsFinalizada ? EstadoCombate.Finalizado : EstadoCombate.EnCurso,
                TurnoPersonaje1 = batalla.EsTurnoDeCombatiente1,
                NumeroTurno = combate.NumeroTurno + 1,
                VidaPersonaje1 = p1.Vida,
                VidaPersonaje2 = p2.Vida,
                RecursoPersonaje1 = CatalogoPersonajes.LeerRecurso(registro1, p1),
                RecursoPersonaje2 = CatalogoPersonajes.LeerRecurso(registro2, p2),
                GanadorId = batalla.Ganador?.Id,
                FechaCreacion = combate.FechaCreacion
            };

            if (!_turnos.Registrar(turno, actualizado, combate.NumeroTurno))
                return Resultado<TurnoEjecutadoRespuesta>.Invalido("El combate cambió mientras se ejecutaba el turno. Volver a intentarlo.");

            var guardado = _turnos.ObtenerPorId(combate.Id, turno.Id, TipoConexion.Administrador) ?? turno;
            return Resultado<TurnoEjecutadoRespuesta>.Ok(new TurnoEjecutadoRespuesta(
                MapeoDtos.Turno(guardado),
                MapeoDtos.CombateEstado(actualizado, registro1, registro2)));
        }

        public Resultado<TurnoRespuesta> ActualizarNota(int combateId, int turnoId, ActualizarNotaSolicitud solicitud)
        {
            if (solicitud?.Nota == null)
                return Resultado<TurnoRespuesta>.Invalido("La nota es obligatoria (enviar \"\" para borrarla).");

            var nota = solicitud.Nota.Trim();
            if (nota.Length > LargoMaximoNota)
                return Resultado<TurnoRespuesta>.Invalido($"La nota no puede superar {LargoMaximoNota} caracteres.");

            if (_combates.ObtenerPorId(combateId, TipoConexion.Administrador) == null)
                return Resultado<TurnoRespuesta>.NoEncontrado($"No existe el combate {combateId}.");

            if (!_turnos.ActualizarNota(combateId, turnoId, nota.Length == 0 ? null : nota))
                return Resultado<TurnoRespuesta>.NoEncontrado($"No existe el turno {turnoId} en el combate {combateId}.");

            var turno = _turnos.ObtenerPorId(combateId, turnoId, TipoConexion.Administrador);
            return turno == null
                ? Resultado<TurnoRespuesta>.NoEncontrado($"No existe el turno {turnoId} en el combate {combateId}.")
                : Resultado<TurnoRespuesta>.Ok(MapeoDtos.Turno(turno));
        }

        public Resultado EliminarUltimo(int combateId, int turnoId)
        {
            var combate = _combates.ObtenerPorId(combateId, TipoConexion.Administrador);
            if (combate == null) return Resultado.NoEncontrado($"No existe el combate {combateId}.");

            var turno = _turnos.ObtenerPorId(combateId, turnoId, TipoConexion.Administrador);
            if (turno == null) return Resultado.NoEncontrado($"No existe el turno {turnoId} en el combate {combateId}.");

            var ultimo = _turnos.ObtenerUltimo(combateId, TipoConexion.Administrador);
            if (ultimo == null || ultimo.Id != turno.Id)
                return Resultado.Conflicto($"El turno {turnoId} no es el último del combate {combateId}.");

            // Estado previo guardado en el turno: vida y recurso antes de la acción, y le vuelve a tocar al atacante.
            var restaurado = new CombateRegistro
            {
                Id = combate.Id,
                Personaje1Id = combate.Personaje1Id,
                Personaje2Id = combate.Personaje2Id,
                Estado = EstadoCombate.EnCurso,
                TurnoPersonaje1 = turno.AtacanteId == combate.Personaje1Id,
                NumeroTurno = turno.Numero,
                VidaPersonaje1 = turno.VidaPersonaje1Antes,
                VidaPersonaje2 = turno.VidaPersonaje2Antes,
                RecursoPersonaje1 = turno.RecursoPersonaje1Antes,
                RecursoPersonaje2 = turno.RecursoPersonaje2Antes,
                GanadorId = null,
                FechaCreacion = combate.FechaCreacion
            };

            return _turnos.EliminarUltimoYRestaurar(turno, restaurado)
                ? Resultado.Ok()
                : Resultado.Conflicto($"El turno {turnoId} dejó de ser el último del combate {combateId}.");
        }
    }
}
