using Persistencia.Repositorios;

namespace Aplicacion.Interfaces
{
    public interface IEstadisticasService
    {
        ResumenEstadisticas ObtenerResumenEstadisticas();
        IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad();
        IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad();
        IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta);
    }
}