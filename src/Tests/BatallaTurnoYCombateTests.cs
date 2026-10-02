using System;
using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaTurnoYCombateTests
    {
        [Theory]
        [InlineData(100, 15, 5, 90, 90)]
        [InlineData(100, 20, 10, 90, 90)]
        [InlineData(100, 30, 5, 75, 75)]
        public void EjecutarTurno_AmbosCombatientesVivos_IncrementaNumeroDeTurnoYContraataca(
            int vidaInicial, int ataque, int defensa, int vidaEsperadaC1, int vidaEsperadaC2)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var c1 = new Guerrero("P1", vidaInicial, ataque, defensa, 0);
            var c2 = new Guerrero("P2", vidaInicial, ataque, defensa, 0);

            personajeRepo.Agregar(c1);
            personajeRepo.Agregar(c2);

            var batalla = service.CrearBatalla(c1, c2);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(vidaEsperadaC1, c1.Vida);
            Assert.Equal(vidaEsperadaC2, c2.Vida);
            Assert.Equal(2, batalla.NumeroTurno);
        }

        [Theory]
        [InlineData(100, 50, 30, 5)]
        [InlineData(150, 60, 50, 10)]
        [InlineData(80, 30, 20, 5)]
        public void EjecutarCombateCompleto_CombateHastaElFinal_RetornaGanadorYFinalizaBatalla(
            int vidaFuerte, int atqFuerte, int vidaDebil, int atqDebil)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var fuerte = new Guerrero("Fuerte", vidaFuerte, atqFuerte, 10, 0);
            var debil = new Guerrero("Debil", vidaDebil, atqDebil, 0, 0);

            personajeRepo.Agregar(fuerte);
            personajeRepo.Agregar(debil);

            var batalla = service.CrearBatalla(fuerte, debil);

            // Act
            Personaje ganador = service.EjecutarCombateCompleto(batalla);

            // Assert
            Assert.Equal(fuerte, ganador);
            Assert.True(batalla.EsFinalizada);
            Assert.False(debil.EstaVivo);
        }

        [Theory]
        [InlineData(0)]   // Caso inválido / límite: Vida en 0
        [InlineData(-5)]  // Vida negativa
        [InlineData(-10)] // Vida altamente negativa
        public void EjecutarTurno_BatallaYaFinalizada_LanzaInvalidOperationException(int vidaDefensaDerrotado)
        {
            // Arrange
            var personajeRepo = new FakePersonajeRepository();
            var batallaRepo = new FakeBatallaRepository();
            var service = new BatallaService(batallaRepo, personajeRepo);

            var c1 = new Guerrero("P1", 100, 10, 0, 0);
            var c2 = new Guerrero("P2", 100, 10, 0, 0);
            c2.Vida = vidaDefensaDerrotado;

            personajeRepo.Agregar(c1);
            personajeRepo.Agregar(c2);

            var batalla = new Batalla(c1, c2);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.EjecutarTurno(batalla));
        }
    }
}