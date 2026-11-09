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
        public void ObtenerRankingPersonajes_RetornaOrdenadoPorVidaDescendente()
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var estadisticasRepo = new FakeEstadisticasRepository();

            var p1 = new Guerrero("Thor", 100, 20, 10, 0) { Vida = 40 };
            var p2 = new Mago("Gandalf", 80, 25, 5, 30) { Vida = 80 };
            personajeRepo.Agregar(p1);
            personajeRepo.Agregar(p2);

            var service = new EstadisticasService(batallaRepo, personajeRepo, estadisticasRepo);

            // Act
            var ranking = service.ObtenerRankingPersonajes().ToList();

            // Assert
            Assert.Equal(2, ranking.Count);
            Assert.StartsWith("Gandalf", ranking[0]);
            Assert.StartsWith("Thor", ranking[1]);
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
    }
}
