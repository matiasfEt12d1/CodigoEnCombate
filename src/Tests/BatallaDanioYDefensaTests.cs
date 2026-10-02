using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaDanioYDefensaTests
    {
        [Theory]
        [InlineData(25, 10, 100, 85)]  // Caso normal
        [InlineData(30, 5, 100, 75)]   // Caso normal con mayor ataque
        [InlineData(10, 15, 100, 99)]  // Caso límite: defensa mayor al ataque (aplica daño mínimo 1)
        public void EjecutarTurno_AtaqueBaseReducidoPorDefensa_RestaVidaCorrectamente(
            int ataque, int defensa, int vidaInicial, int vidaEsperada)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var atacante = new Guerrero("Atacante", 100, ataque, 5, 0);
            var defensor = new Guerrero("Defensor", vidaInicial, 10, defensa, 0);

            personajeRepo.Agregar(atacante);
            personajeRepo.Agregar(defensor);

            var batalla = service.CrearBatalla(atacante, defensor);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(vidaEsperada, defensor.Vida);
        }

        [Theory]
        [InlineData(20, 5, 10, 100, 0, 95)]  // Escudo absorbido completamente
        [InlineData(12, 5, 10, 100, 3, 100)] // Escudo absorbido parcialmente (sin daño a vida)
        [InlineData(35, 5, 10, 100, 0, 80)]  // Ataque alto rompe escudo y resta vida
        public void EjecutarTurno_AtaqueAGuerreroConEscudo_AbsorbeDanioEnEscudoAntesDeVida(
            int ataque, int defensa, int escudoInicial, int vidaInicial, int escudoEsperado, int vidaEsperada)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var atacante = new Guerrero("Atacante", 100, ataque, 0, 0);
            var defensor = new Guerrero("GuerreroEscudo", vidaInicial, 10, defensa, escudoInicial);

            personajeRepo.Agregar(atacante);
            personajeRepo.Agregar(defensor);

            var batalla = service.CrearBatalla(atacante, defensor);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(escudoEsperado, defensor.Escudo);
            Assert.Equal(vidaEsperada, defensor.Vida);
        }
    }
}