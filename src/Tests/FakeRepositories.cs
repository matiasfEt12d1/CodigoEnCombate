using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Tests
{
    public class FakePersonajeRepository : IPersonajeRepository
    {
        private readonly List<Personaje> _personajes = new();

        public Personaje? ObtenerPorId(int id) => _personajes.FirstOrDefault(p => p.Id == id);
        public IEnumerable<Personaje> ObtenerTodos() => _personajes;
        public int Agregar(Personaje entidad)
        {
            entidad.Id = _personajes.Count + 1;
            _personajes.Add(entidad);
            return entidad.Id;
        }
        public bool Actualizar(Personaje entidad) => true;
        public bool Eliminar(int id) => true;
        public Personaje? ObtenerPorNombre(string nombre) => _personajes.FirstOrDefault(p => p.Nombre == nombre);
        public IEnumerable<Personaje> ObtenerPorTipo(string tipo) => 
            _personajes.Where(p => p.GetType().Name.Equals(tipo, StringComparison.OrdinalIgnoreCase));
        public bool AgregarHabilidad(int personajeId, Habilidad habilidad) => true;
    }

    public class FakeBatallaRepository : IBatallaRepository
    {
        private readonly List<Batalla> _batallas = new();

        public Batalla? ObtenerPorId(int id) => _batallas.FirstOrDefault(b => b.Id == id);
        public IEnumerable<Batalla> ObtenerTodos() => _batallas;
        public int Agregar(Batalla entidad)
        {
            entidad.Id = _batallas.Count + 1;
            _batallas.Add(entidad);
            return entidad.Id;
        }
        public bool Actualizar(Batalla entidad) => true;
        public bool Eliminar(int id) => true;
        public IEnumerable<Batalla> ObtenerBatallasActivas() => _batallas.Where(b => !b.EsFinalizada);
        
        public List<(int BatallaId, int Turno, int AtacanteId, int DefensorId, int Dano, int? HabilidadId)> HistorialRegistrado { get; } = new();

        public bool FinalizarBatalla(int batallaId, int? ganadorId, int numeroTurno, int vidaC1, int vidaC2)
        {
            var batalla = ObtenerPorId(batallaId);
            if (batalla != null)
            {
                _batallas.Remove(batalla);
            }
            return true;
        }

        public void ProcesarAccionCombate(int batallaId, int atacanteId, int defensorId, int danoRealizado, int nuevaVidaDefensor, int? habilidadId, int numeroTurno)
        {
            HistorialRegistrado.Add((batallaId, numeroTurno, atacanteId, defensorId, danoRealizado, habilidadId));
        }

        public void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, int? habilidadId, int danoCausado, int nuevaVidaDefensor)
        {
            HistorialRegistrado.Add((batallaId, numeroTurno, atacanteId, defensorId, danoCausado, habilidadId));
        }

        public IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta)
        {
            return Enumerable.Empty<RankingTipoPersonajeDto>();
        }

        public IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta)
        {
            return Enumerable.Empty<ReporteBatallaDetalladoDto>();
        }
    }

    public class FakeEstadisticasRepository : IEstadisticasRepository
    {
        public virtual IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta)
            => Enumerable.Empty<ResumenBatallasPeriodoTipoDto>();

        public virtual IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta)
            => Enumerable.Empty<VictoriasDerrotasTipoPeriodoDto>();

        public virtual IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad()
            => Enumerable.Empty<RankingPersonajeEfectividadDto>();

        public virtual IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad()
            => Enumerable.Empty<DanoPromedioClaseHabilidadDto>();

        public virtual IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta)
            => Enumerable.Empty<DanoTotalPorPeriodoDto>();

        public virtual IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta)
            => Enumerable.Empty<RankingTipoPersonajeDto>();

        public virtual IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta)
            => Enumerable.Empty<ReporteBatallaDetalladoDto>();

        public virtual ResumenGeneralBatallasDto ObtenerResumenGeneralBatallas()
            => new ResumenGeneralBatallasDto();
    }
}
