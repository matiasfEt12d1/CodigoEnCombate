using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaDanioYDefensaTests
    {
        [Fact]
        public void EjecutarTurno_AtaqueBaseReducidoPorDefensa_RestaVidaCorrectamente()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var atacante = new Guerrero("Atacante", 100, 25, 5, 0);
            var defensor = new Guerrero("Defensor", 100, 10, 10, 0);
            var batalla = service.CrearBatalla(atacante, defensor);

            // Act
            service.EjecutarTurno(batalla);

            // Assert (Daño = 25 - 10 = 15; Vida esperada = 85)
            Assert.Equal(85, defensor.Vida);
        }

        [Fact]
        public void EjecutarTurno_AtaqueAGuerreroConEscudo_AbsorbeDanioEnEscudoAntesDeVida()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var atacante = new Guerrero("Atacante", 100, 20, 0, 0);
            var defensor = new Guerrero("GuerreroEscudo", 100, 10, 5, 10);
            var batalla = service.CrearBatalla(atacante, defensor);

            // Act
            service.EjecutarTurno(batalla);

            // Assert (Daño = 20 - 5 = 15; Escudo absorbe 10 y pasa a 0; Restante 5 a Vida = 95)
            Assert.Equal(0, defensor.Escudo);
            Assert.Equal(95, defensor.Vida);
        }
    }
}