using Persistencia;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Tests
{
    // Tablas de combate_api en memoria para probar los servicios de la API sin MySQL.
    // Los repositorios guardan y devuelven copias, como lo haría una base real.
    public class BaseEnMemoria
    {
        public List<PersonajeRegistro> Personajes { get; } = new();
        public List<CombateRegistro> Combates { get; } = new();
        public List<TurnoRegistro> Turnos { get; } = new();

        public FakePersonajeApiRepository PersonajesRepo { get; }
        public FakeCombateRepository CombatesRepo { get; }
        public FakeTurnoRepository TurnosRepo { get; }

        public BaseEnMemoria()
        {
            PersonajesRepo = new FakePersonajeApiRepository(this);
            CombatesRepo = new FakeCombateRepository(this);
            TurnosRepo = new FakeTurnoRepository(this);
        }

        public static CombateRegistro Copiar(CombateRegistro c) => new()
        {
            Id = c.Id,
            Personaje1Id = c.Personaje1Id,
            Personaje2Id = c.Personaje2Id,
            Estado = c.Estado,
            TurnoPersonaje1 = c.TurnoPersonaje1,
            NumeroTurno = c.NumeroTurno,
            VidaPersonaje1 = c.VidaPersonaje1,
            VidaPersonaje2 = c.VidaPersonaje2,
            RecursoPersonaje1 = c.RecursoPersonaje1,
            RecursoPersonaje2 = c.RecursoPersonaje2,
            GanadorId = c.GanadorId,
            FechaCreacion = c.FechaCreacion
        };

        public static TurnoRegistro Copiar(TurnoRegistro t) => new()
        {
            Id = t.Id,
            CombateId = t.CombateId,
            Numero = t.Numero,
            AtacanteId = t.AtacanteId,
            DefensorId = t.DefensorId,
            Accion = t.Accion,
            Danio = t.Danio,
            Nota = t.Nota,
            VidaPersonaje1Antes = t.VidaPersonaje1Antes,
            VidaPersonaje2Antes = t.VidaPersonaje2Antes,
            RecursoPersonaje1Antes = t.RecursoPersonaje1Antes,
            RecursoPersonaje2Antes = t.RecursoPersonaje2Antes,
            Fecha = t.Fecha
        };
    }

    public class FakePersonajeApiRepository : IPersonajeApiRepository
    {
        private readonly BaseEnMemoria _base;

        public FakePersonajeApiRepository(BaseEnMemoria baseEnMemoria) => _base = baseEnMemoria;

        public IEnumerable<PersonajeRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.ApiInvitado) => _base.Personajes.ToList();

        public PersonajeRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.ApiInvitado) =>
            _base.Personajes.FirstOrDefault(p => p.Id == id);

        public int Agregar(PersonajeRegistro personaje)
        {
            personaje.Id = _base.Personajes.Count == 0 ? 1 : _base.Personajes.Max(p => p.Id) + 1;
            _base.Personajes.Add(personaje);
            return personaje.Id;
        }

        public bool ActualizarNombre(int id, string nombre)
        {
            var personaje = ObtenerPorId(id);
            if (personaje == null) return false;
            personaje.Nombre = nombre;
            return true;
        }

        public bool EliminarSiNoParticipo(int id)
        {
            if (_base.Combates.Any(c => c.Participa(id))) return false;
            return _base.Personajes.RemoveAll(p => p.Id == id) > 0;
        }
    }

    public class FakeCombateRepository : ICombateRepository
    {
        private readonly BaseEnMemoria _base;

        public FakeCombateRepository(BaseEnMemoria baseEnMemoria) => _base = baseEnMemoria;

        public IEnumerable<CombateRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.ApiInvitado) =>
            _base.Combates.Select(BaseEnMemoria.Copiar).ToList();

        public IEnumerable<CombateRegistro> ObtenerPorPersonaje(int personajeId, TipoConexion conexion = TipoConexion.ApiInvitado) =>
            _base.Combates.Where(c => c.Participa(personajeId)).Select(BaseEnMemoria.Copiar).ToList();

        public CombateRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            var combate = _base.Combates.FirstOrDefault(c => c.Id == id);
            return combate == null ? null : BaseEnMemoria.Copiar(combate);
        }

        public bool TieneTurnos(int id, TipoConexion conexion = TipoConexion.ApiInvitado) => _base.Turnos.Any(t => t.CombateId == id);

        public int Agregar(CombateRegistro combate)
        {
            combate.Id = _base.Combates.Count == 0 ? 1 : _base.Combates.Max(c => c.Id) + 1;
            _base.Combates.Add(BaseEnMemoria.Copiar(combate));
            return combate.Id;
        }

        public bool ActualizarParticipantesSiNoTieneTurnos(CombateRegistro combate)
        {
            if (TieneTurnos(combate.Id)) return false;
            return Reemplazar(combate);
        }

        public bool EliminarSiNoTieneTurnos(int id)
        {
            if (TieneTurnos(id)) return false;
            return _base.Combates.RemoveAll(c => c.Id == id) > 0;
        }

        internal bool Reemplazar(CombateRegistro combate)
        {
            int indice = _base.Combates.FindIndex(c => c.Id == combate.Id);
            if (indice < 0) return false;
            _base.Combates[indice] = BaseEnMemoria.Copiar(combate);
            return true;
        }
    }

    public class FakeTurnoRepository : ITurnoRepository
    {
        private readonly BaseEnMemoria _base;

        public FakeTurnoRepository(BaseEnMemoria baseEnMemoria) => _base = baseEnMemoria;

        public IEnumerable<TurnoRegistro> ObtenerPorCombate(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado) =>
            _base.Turnos.Where(t => t.CombateId == combateId).OrderBy(t => t.Numero).Select(BaseEnMemoria.Copiar).ToList();

        public TurnoRegistro? ObtenerPorId(int combateId, int turnoId, TipoConexion conexion = TipoConexion.ApiInvitado)
        {
            var turno = _base.Turnos.FirstOrDefault(t => t.CombateId == combateId && t.Id == turnoId);
            return turno == null ? null : BaseEnMemoria.Copiar(turno);
        }

        public TurnoRegistro? ObtenerUltimo(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado) =>
            ObtenerPorCombate(combateId).LastOrDefault();

        public bool Registrar(TurnoRegistro turno, CombateRegistro combateActualizado, int numeroTurnoEsperado)
        {
            var actual = _base.Combates.FirstOrDefault(c => c.Id == combateActualizado.Id);
            if (actual == null || actual.NumeroTurno != numeroTurnoEsperado) return false;

            turno.Id = _base.Turnos.Count == 0 ? 1 : _base.Turnos.Max(t => t.Id) + 1;
            _base.Turnos.Add(BaseEnMemoria.Copiar(turno));
            return _base.CombatesRepo.Reemplazar(combateActualizado);
        }

        public bool ActualizarNota(int combateId, int turnoId, string? nota)
        {
            var turno = _base.Turnos.FirstOrDefault(t => t.CombateId == combateId && t.Id == turnoId);
            if (turno == null) return false;
            turno.Nota = nota;
            return true;
        }

        public bool EliminarUltimoYRestaurar(TurnoRegistro turno, CombateRegistro combateRestaurado)
        {
            var ultimo = ObtenerUltimo(turno.CombateId);
            if (ultimo == null || ultimo.Id != turno.Id) return false;

            _base.Turnos.RemoveAll(t => t.Id == turno.Id);
            return _base.CombatesRepo.Reemplazar(combateRestaurado);
        }
    }
}
