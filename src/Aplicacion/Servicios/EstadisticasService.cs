using Aplicacion.Interfaces;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class EstadisticasService : IEstadisticasService
    {
        private readonly IBatallaRepository _batallaRepository;
        private readonly IPersonajeRepository _personajeRepository;
        private readonly IEstadisticasRepository? _estadisticasRepository;

        public EstadisticasService(IBatallaRepository batallaRepository, IPersonajeRepository personajeRepository)
            : this(batallaRepository, personajeRepository, null)
        {
        }

        public EstadisticasService(
            IBatallaRepository batallaRepository, 
            IPersonajeRepository personajeRepository,
            IEstadisticasRepository? estadisticasRepository)
        {
            _batallaRepository = batallaRepository ?? throw new ArgumentNullException(nameof(batallaRepository));
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
            _estadisticasRepository = estadisticasRepository;
        }

        public ResumenEstadisticas ObtenerResumenEstadisticas()
        {
            if (_estadisticasRepository != null)
            {
                var resumen = _estadisticasRepository.ObtenerResumenGeneralBatallas();
                return new ResumenEstadisticas
                {
                    TotalBatallas = resumen.TotalBatallas,
                    BatallasFinalizadas = resumen.BatallasFinalizadas,
                    BatallasActivas = resumen.BatallasActivas
                };
            }

            var todas = _batallaRepository.ObtenerTodos().ToList();

            return new ResumenEstadisticas
            {
                TotalBatallas = todas.Count,
                BatallasFinalizadas = todas.Count(b => b.EsFinalizada),
                BatallasActivas = todas.Count(b => !b.EsFinalizada)
            };
        }

        public IEnumerable<string> ObtenerRankingPersonajes()
        {
            var personajes = _personajeRepository.ObtenerTodos();
            return personajes
                .OrderByDescending(p => p.Vida)
                .Select(p => $"{p.Nombre} (Vida actual: {p.Vida}/{p.VidaMax})");
        }

        public IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (_estadisticasRepository == null)
                throw new InvalidOperationException("No se ha configurado el repositorio de estadísticas.");

            return _estadisticasRepository.ObtenerResumenBatallasPorPeriodoYTipo(fechaDesde, fechaHasta);
        }

        public IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (_estadisticasRepository == null)
                throw new InvalidOperationException("No se ha configurado el repositorio de estadísticas.");

            return _estadisticasRepository.ObtenerVictoriasYDerrotasPorTipoYPeriodo(fechaDesde, fechaHasta);
        }

        public IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad()
        {
            if (_estadisticasRepository == null)
                throw new InvalidOperationException("No se ha configurado el repositorio de estadísticas.");

            return _estadisticasRepository.ObtenerRankingPersonajesEfectividad();
        }

        public IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad()
        {
            if (_estadisticasRepository == null)
                throw new InvalidOperationException("No se ha configurado el repositorio de estadísticas.");

            return _estadisticasRepository.ObtenerDanoPromedioPorClaseYHabilidad();
        }

        public IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            if (_estadisticasRepository == null)
                throw new InvalidOperationException("No se ha configurado el repositorio de estadísticas.");

            return _estadisticasRepository.ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(fechaDesde, fechaHasta);
        }
    }
}