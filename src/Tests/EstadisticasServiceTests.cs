using Aplicacion.Servicios;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Tests
{
    public class EstadisticasServiceTests
    {
        [Fact]
        public void ObtenerResumenEstadisticas_ConRepositorioEstadisticas_RetornaResumenCorrecto()
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var estadisticasRepo = new TestableFakeEstadisticasRepository(new ResumenGeneralBatallasDto
            {
                TotalBatallas = 10,
                BatallasFinalizadas = 7,
                BatallasActivas = 3
            });

            var service = new EstadisticasService(batallaRepo, personajeRepo, estadisticasRepo);

            // Act
            var resumen = service.ObtenerResumenEstadisticas();

            // Assert
            Assert.Equal(10, resumen.TotalBatallas);
            Assert.Equal(7, resumen.BatallasFinalizadas);
            Assert.Equal(3, resumen.BatallasActivas);
        }

        [Fact]
        public void ObtenerRankingPorTipoPersonaje_DevuelveElRankingDelRepositorio()
        {
            // Arrange
            var estadisticasRepo = new RankingFakeEstadisticasRepository();
            var service = new EstadisticasService(new FakeBatallaRepository(), new FakePersonajeRepository(), estadisticasRepo);
            var desde = new DateTime(2026, 1, 1);
            var hasta = new DateTime(2026, 12, 31);

            // Act
            var ranking = service.ObtenerRankingPorTipoPersonaje(desde, hasta).ToList();

            // Assert
            Assert.Equal("Guerrero", ranking.Single().TipoPersonaje);
            Assert.Equal((desde, hasta), estadisticasRepo.PeriodoConsultado);
        }

        [Fact]
        public void GenerarReporteBatallasDetallado_DevuelveElReporteDelRepositorio()
        {
            // Arrange
            var estadisticasRepo = new RankingFakeEstadisticasRepository();
            var service = new EstadisticasService(new FakeBatallaRepository(), new FakePersonajeRepository(), estadisticasRepo);

            // Act
            var reporte = service.GenerarReporteBatallasDetallado(DateTime.MinValue, DateTime.MaxValue).ToList();

            // Assert
            Assert.Equal(1, reporte.Single().BatallaId);
        }

        [Fact]
        public void ObtenerRankingPorTipoPersonaje_SinRepositorioDeEstadisticas_LanzaExcepcion()
        {
            // Arrange
            var service = new EstadisticasService(new FakeBatallaRepository(), new FakePersonajeRepository());

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.ObtenerRankingPorTipoPersonaje(DateTime.MinValue, DateTime.MaxValue));
        }

        private class TestableFakeEstadisticasRepository : FakeEstadisticasRepository
        {
            private readonly ResumenGeneralBatallasDto _resumen;

            public TestableFakeEstadisticasRepository(ResumenGeneralBatallasDto resumen)
            {
                _resumen = resumen;
            }

            public override ResumenGeneralBatallasDto ObtenerResumenGeneralBatallas() => _resumen;
        }

        private class RankingFakeEstadisticasRepository : FakeEstadisticasRepository
        {
            public (DateTime Desde, DateTime Hasta)? PeriodoConsultado { get; private set; }

            public override IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta)
            {
                PeriodoConsultado = (fechaDesde, fechaHasta);
                return new[] { new RankingTipoPersonajeDto { TipoPersonaje = "Guerrero", TotalVictorias = 3 } };
            }

            public override IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta)
                => new[] { new ReporteBatallaDetalladoDto { BatallaId = 1 } };
        }
    }
}
