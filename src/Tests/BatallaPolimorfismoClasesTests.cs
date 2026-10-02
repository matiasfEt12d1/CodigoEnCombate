using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaPolimorfismoClasesTests
    {
        [Fact]
        public void EjecutarTurno_MagoConManaSuficiente_AplicaDanioExtraYConsumeMana()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var mago = new Mago("Merlin", 100, 15, 5, 20);
            var objetivo = new Guerrero("Objetivo", 100, 5, 5, 0);
            var batalla = service.CrearBatalla(mago, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert (Ataque total = 15 + 15 = 30; Daño = 30 - 5 = 25; Maná = 10)
            Assert.Equal(10, mago.Mana);
            Assert.Equal(75, objetivo.Vida);
        }

        [Fact]
        public void EjecutarTurno_ArqueroSinFlechas_ReduceAtaqueALaMitad()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var arquero = new Arquero("Robin", 100, 20, 5, 0);
            var objetivo = new Guerrero("Objetivo", 100, 5, 2, 0);
            var batalla = service.CrearBatalla(arquero, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert (Ataque penalizado = 10; Daño = 10 - 2 = 8; Vida esperada = 92)
            Assert.Equal(92, objetivo.Vida);
        }

        [Fact]
        public void EjecutarTurno_AsesinoConCriticoGarantizado_DuplicaAtaqueBase()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var asesino = new Asesino("Sombra", 100, 20, 5, 1.0);
            var objetivo = new Guerrero("Objetivo", 100, 5, 10, 0);
            var batalla = service.CrearBatalla(asesino, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert (Ataque crítico = 40; Daño = 40 - 10 = 30; Vida esperada = 70)
            Assert.Equal(70, objetivo.Vida);
        }
    }
}