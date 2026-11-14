namespace Persistencia.Repositorios
{
    public interface IEstadisticasRepository
    {
        IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad();
        IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad();
        IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta);
        IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta);
        ResumenGeneralBatallasDto ObtenerResumenGeneralBatallas();
    }

    public class ResumenBatallasPeriodoTipoDto
    {
        public string TipoPersonaje { get; set; } = string.Empty;
        public int TotalBatallas { get; set; }
        public int BatallasFinalizadas { get; set; }
        public int BatallasEnProgreso { get; set; }
    }

    public class VictoriasDerrotasTipoPeriodoDto
    {
        public string TipoPersonaje { get; set; } = string.Empty;
        public int TotalBatallas { get; set; }
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public int Empates { get; set; }
    }

    public class RankingPersonajeEfectividadDto
    {
        public int PersonajeId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int TotalBatallas { get; set; }
        public int Victorias { get; set; }
        public int Derrotas { get; set; }
        public decimal PorcentajeEfectividad { get; set; }
    }

    public class DanoPromedioClaseHabilidadDto
    {
        public string ClasePersonaje { get; set; } = string.Empty;
        public string Habilidad { get; set; } = string.Empty;
        public int CantidadUsos { get; set; }
        public decimal DanoPromedio { get; set; }
        public int DanoMaximo { get; set; }
    }

    public class DanoTotalPorPeriodoDto
    {
        public int PersonajeId { get; set; }
        public string NombrePersonaje { get; set; } = string.Empty;
        public string TipoPersonaje { get; set; } = string.Empty;
        public decimal DanoTotalRealizado { get; set; }
        public decimal DanoTotalRecibido { get; set; }
    }

    public class ResumenGeneralBatallasDto
    {
        public int TotalBatallas { get; set; }
        public int BatallasFinalizadas { get; set; }
        public int BatallasActivas { get; set; }
    }

    public class RankingTipoPersonajeDto
    {
        public string TipoPersonaje { get; set; } = string.Empty;
        public int TotalBatallas { get; set; }
        public int TotalVictorias { get; set; }
        public int TotalDerrotas { get; set; }
        public decimal DanoPromedioPorAccion { get; set; }
    }

    public class ReporteBatallaDetalladoDto
    {
        public int BatallaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public int TurnosTotales { get; set; }
        public bool Finalizada { get; set; }
        public string Combatiente1_Nombre { get; set; } = string.Empty;
        public string Combatiente1_Tipo { get; set; } = string.Empty;
        public string Combatiente2_Nombre { get; set; } = string.Empty;
        public string Combatiente2_Tipo { get; set; } = string.Empty;
        public string Ganador_Nombre { get; set; } = string.Empty;
        public string Ganador_Tipo { get; set; } = string.Empty;
    }
}
