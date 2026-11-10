using Persistencia.Repositorios;

namespace Aplicacion.Interfaces
{
    public interface IEstadisticasService
    {
        ResumenEstadisticas ObtenerResumenEstadisticas();
        IEnumerable<string> ObtenerRankingPersonajes();
        IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad();
        IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad();
        IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta);
    }
}