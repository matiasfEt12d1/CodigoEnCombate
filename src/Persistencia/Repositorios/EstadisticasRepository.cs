namespace Persistencia.Repositorios
{
    public class EstadisticasRepository : IEstadisticasRepository
    {
        private readonly IDapperContext _context;

        public EstadisticasRepository(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IEnumerable<ResumenBatallasPeriodoTipoDto> ObtenerResumenBatallasPorPeriodoYTipo(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_ResumenBatallasPorPeriodoYTipo(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<ResumenBatallasPeriodoTipoDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public IEnumerable<VictoriasDerrotasTipoPeriodoDto> ObtenerVictoriasYDerrotasPorTipoYPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_VictoriasYDerrotasPorTipoYPeriodo(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<VictoriasDerrotasTipoPeriodoDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public IEnumerable<RankingPersonajeEfectividadDto> ObtenerRankingPersonajesEfectividad()
        {
            const string sql = "CALL sp_RankingPersonajesEfectividad();";

            return _context.Query<RankingPersonajeEfectividadDto>(sql);
        }

        public IEnumerable<DanoPromedioClaseHabilidadDto> ObtenerDanoPromedioPorClaseYHabilidad()
        {
            const string sql = "CALL sp_DanoPromedioPorClaseYHabilidad();";

            return _context.Query<DanoPromedioClaseHabilidadDto>(sql);
        }

        public IEnumerable<DanoTotalPorPeriodoDto> ObtenerDanoTotalRealizadoYRecibidoPorPeriodo(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_DanoTotalRealizadoYRecibidoPorPeriodo(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<DanoTotalPorPeriodoDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_ObtenerRankingPorTipoPersonaje(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<RankingTipoPersonajeDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_GenerarReporteBatallasDetallado(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<ReporteBatallaDetalladoDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public ResumenGeneralBatallasDto ObtenerResumenGeneralBatallas()
        {
            const string sql = @"
                SELECT 
                    COUNT(*) AS TotalBatallas,
                    COALESCE(SUM(CASE WHEN es_finalizada = 1 THEN 1 ELSE 0 END), 0) AS BatallasFinalizadas,
                    COALESCE(SUM(CASE WHEN es_finalizada = 0 THEN 1 ELSE 0 END), 0) AS BatallasActivas
                FROM Batallas;";

            return _context.Query<ResumenGeneralBatallasDto>(sql).FirstOrDefault() 
                   ?? new ResumenGeneralBatallasDto();
        }
    }
}
