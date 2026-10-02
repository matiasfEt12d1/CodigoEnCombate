using System;
using Xunit;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    public class BatallaTurnoYCombateTests
    {
        [Fact]
        public void EjecutarTurno_AmbosCombatientesVivos_IncrementaNumeroDeTurnoYContraataca()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var c1 = new Guerrero("P1", 100, 15, 5, 0);
            var c2 = new Guerrero("P2", 100, 15, 5, 0);
            var batalla = service.CrearBatalla(c1, c2);

            // Act
            service.EjecutarTurno(batalla);

            // Assert
            Assert.Equal(90, c1.Vida);
            Assert.Equal(90, c2.Vida);
            Assert.Equal(2, batalla.NumeroTurno);
        }

        [Fact]
        public void EjecutarCombateCompleto_CombateHastaElFinal_RetornaGanadorYFinalizaBatalla()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var fuerte = new Guerrero("Fuerte", 100, 50, 10, 0);
            var debil = new Guerrero("Debil", 30, 5, 0, 0);
            var batalla = service.CrearBatalla(fuerte, debil);

            // Act
            Personaje ganador = service.EjecutarCombateCompleto(batalla);

            // Assert
            Assert.Equal(fuerte, ganador);
            Assert.True(batalla.EsFinalizada);
            Assert.False(debil.EstaVivo);
        }

        [Fact]
        public void EjecutarTurno_BatallaYaFinalizada_LanzaInvalidOperationException()
        {
            // Arrange
            var service = new BatallaService(new FakeBatallaRepository(), new FakePersonajeRepository());
            var c1 = new Guerrero("P1", 100, 10, 0, 0);
            var c2 = new Guerrero("P2", 100, 10, 0, 0);
            c2.Vida = 0; // Personaje derrotado
            var batalla = new Batalla(c1, c2);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.EjecutarTurno(batalla));
        }
    }
}