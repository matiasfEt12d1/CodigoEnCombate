using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaPolimorfismoClasesTests
    {
        [Theory]
        [InlineData(20, 15, 5, 10, 75)] // Caso normal con maná suficiente
        [InlineData(30, 20, 10, 20, 75)] // Caso con mayor ataque y defensa
        [InlineData(10, 25, 10, 0, 70)]  // Caso límite: maná justo (queda en 0)
        public void EjecutarTurno_MagoConManaSuficiente_AplicaDanioExtraYConsumeMana(
            int manaInicial, int ataque, int defensaObjetivo, int manaEsperado, int vidaEsperadaObjetivo)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var mago = new Mago("Merlin", 100, ataque, 5, manaInicial);
            var objetivo = new Guerrero("Objetivo", 100, 5, defensaObjetivo, 0);

            personajeRepo.Agregar(mago);
            personajeRepo.Agregar(objetivo);

            var batalla = service.CrearBatalla(mago, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(manaEsperado, mago.Mana);
            Assert.Equal(vidaEsperadaObjetivo, objetivo.Vida);
        }

        [Theory]
        [InlineData(20, 2, 92)]  // Caso normal con ataque reducido
        [InlineData(30, 5, 90)]  // Caso con mayor ataque base
        [InlineData(40, 10, 90)] // Caso con mayor defensa del objetivo
        public void EjecutarTurno_ArqueroSinFlechas_ReduceAtaqueALaMitad(
            int ataqueArquero, int defensaObjetivo, int vidaEsperadaObjetivo)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var arquero = new Arquero("Robin", 100, ataqueArquero, 5, 0);
            var objetivo = new Guerrero("Objetivo", 100, 5, defensaObjetivo, 0);

            personajeRepo.Agregar(arquero);
            personajeRepo.Agregar(objetivo);

            var batalla = service.CrearBatalla(arquero, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(vidaEsperadaObjetivo, objetivo.Vida);
        }

        [Theory]
        [InlineData(20, 10, 70)] // Caso normal con crítico
        [InlineData(15, 5, 75)]  // Caso con valores bajos
        [InlineData(25, 0, 50)]  // Caso límite: objetivo sin defensa
        public void EjecutarTurno_AsesinoConCriticoGarantizado_DuplicaAtaqueBase(
            int ataqueAsesino, int defensaObjetivo, int vidaEsperadaObjetivo)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var asesino = new Asesino("Sombra", 100, ataqueAsesino, 5, 1.0);
            var objetivo = new Guerrero("Objetivo", 100, 5, defensaObjetivo, 0);

            personajeRepo.Agregar(asesino);
            personajeRepo.Agregar(objetivo);

            var batalla = service.CrearBatalla(asesino, objetivo);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(vidaEsperadaObjetivo, objetivo.Vida);
        }
    }
}