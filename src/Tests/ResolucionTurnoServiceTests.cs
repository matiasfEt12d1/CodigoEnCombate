using Aplicacion;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    // Acciones y daño registrado por ResolucionTurnoService, sin repositorios ni base de datos.
    public class ResolucionTurnoServiceTests
    {
        private readonly ResolucionTurnoService _service = new();

        private static Guerrero GuerreroConHabilidad(int escudo = 20)
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, escudo);
            guerrero.AgregarHabilidad(new Habilidad("Embestida", 0, 20) { Id = 7 });
            return guerrero;
        }

        [Fact]
        public void ResolverTurno_AtaqueBasico_NoRegistraHabilidad()
        {
            var batalla = new Batalla(GuerreroConHabilidad(), new Mago("Merlina", 80, 14, 4, 60));

            var turno = _service.ResolverTurno(batalla);

            Assert.All(turno.Acciones, a => Assert.Equal(AccionCombate.Atacar, a.Accion));
            Assert.All(turno.Acciones, a => Assert.Null(a.HabilidadId));
        }

        [Fact]
        public void ResolverTurno_Habilidad_UsaLaHabilidadYRegistraSuId()
        {
            var guerrero = GuerreroConHabilidad(escudo: 20);
            var mago = new Mago("Merlina", 80, 14, 4, 60);
            var batalla = new Batalla(guerrero, mago);

            var turno = _service.ResolverTurno(batalla, AccionCombate.Habilidad, AccionCombate.Atacar);

            var embestida = turno.Acciones[0];
            Assert.Equal(AccionCombate.Habilidad, embestida.Accion);
            Assert.Equal(7, embestida.HabilidadId);
            Assert.Equal(34, embestida.Danio); // 18 + 20 - 4
            Assert.Equal(0, guerrero.Escudo);
            Assert.Null(turno.Acciones[1].HabilidadId);
        }

        [Fact]
        public void ResolverTurno_HabilidadSinRegistroGuardado_NoInventaUnId()
        {
            var mago = new Mago("Merlina", 80, 14, 4, 60);
            var batalla = new Batalla(mago, new Guerrero("Thorin", 120, 18, 8, 0));

            var turno = _service.ResolverTurno(batalla, AccionCombate.Habilidad);

            Assert.Equal(AccionCombate.Habilidad, turno.Acciones[0].Accion);
            Assert.Null(turno.Acciones[0].HabilidadId);
            Assert.Equal(28, turno.Acciones[0].Danio); // Bola de fuego: 14 * 2
        }

        [Fact]
        public void ResolverTurno_DanioAbsorbidoPorElEscudo_CuentaComoDanio()
        {
            var asesino = new Asesino("Sombra", 85, 20, 4, 0.0);
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 20);
            var batalla = new Batalla(asesino, guerrero);

            var turno = _service.ResolverTurno(batalla);

            Assert.Equal(12, turno.Acciones[0].Danio); // 20 - 8, todo al escudo
            Assert.Equal(120, guerrero.Vida);
            Assert.Equal(8, guerrero.Escudo);
        }

        [Fact]
        public void ResolverTurno_DefensorDerrotado_NoContraatacaYFinaliza()
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 0);
            var mago = new Mago("Merlina", 80, 14, 4, 60) { Vida = 10 };
            var batalla = new Batalla(guerrero, mago);

            var turno = _service.ResolverTurno(batalla);

            Assert.Single(turno.Acciones);
            Assert.True(turno.Finalizada);
            Assert.Same(guerrero, turno.Ganador);
            Assert.Equal(1, turno.NumeroTurno);
            Assert.Equal(1, batalla.NumeroTurno);
        }

        [Fact]
        public void ResolverTurno_DevuelveElNumeroDelTurnoJugado()
        {
            var batalla = new Batalla(new Guerrero("A", 120, 18, 8, 0), new Guerrero("B", 120, 18, 8, 0));

            var primero = _service.ResolverTurno(batalla);
            var segundo = _service.ResolverTurno(batalla);

            Assert.Equal(1, primero.NumeroTurno);
            Assert.Equal(2, segundo.NumeroTurno);
            Assert.Equal(3, batalla.NumeroTurno);
        }

        [Fact]
        public void ResolverTurno_HabilidadSinRecursos_LanzaExcepcion()
        {
            var arquero = new Arquero("Legolia", 90, 16, 5, 1);
            var batalla = new Batalla(arquero, new Guerrero("Thorin", 120, 18, 8, 0));

            Assert.Throws<InvalidOperationException>(() => _service.ResolverTurno(batalla, AccionCombate.Habilidad));
        }

        [Fact]
        public void ResolverTurno_SegundoSinRecursos_NoAplicaNingunaAccion()
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 0);
            var arquero = new Arquero("Legolia", 90, 16, 5, 1);
            var batalla = new Batalla(guerrero, arquero);

            Assert.Throws<InvalidOperationException>(() =>
                _service.ResolverTurno(batalla, AccionCombate.Atacar, AccionCombate.Habilidad));

            Assert.Equal(90, arquero.Vida);
            Assert.Equal(1, arquero.CantidadFlechas);
            Assert.Equal(1, batalla.NumeroTurno);
        }

        [Fact]
        public void Restaurar_DejaAlPersonajeListoParaOtraBatalla()
        {
            var mago = new Mago("Merlina", 80, 14, 4, 60) { Vida = 0, Mana = 5 };

            mago.Restaurar();

            Assert.Equal(80, mago.Vida);
            Assert.Equal(60, mago.Mana);
            Assert.True(mago.EstaVivo);
        }
    }
}
