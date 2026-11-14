using Aplicacion;
using Aplicacion.Servicios;
using Persistencia.Entidades;

namespace Tests
{
    // Lo que BatallaService registra en el repositorio al crear batallas y ejecutar turnos.
    public class BatallaServiceTests
    {
        private readonly FakePersonajeRepository _personajes = new();
        private readonly FakeBatallaRepository _batallas = new();
        private readonly BatallaService _service;

        public BatallaServiceTests()
        {
            _service = new BatallaService(_batallas, _personajes, new ResolucionTurnoService());
        }

        private Guerrero GuerreroRegistrado(int escudo = 20)
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, escudo);
            guerrero.AgregarHabilidad(new Habilidad("Embestida", 0, 20) { Id = 7 });
            _personajes.Agregar(guerrero);
            return guerrero;
        }

        private Mago MagoRegistrado()
        {
            var mago = new Mago("Merlina", 80, 14, 4, 60);
            _personajes.Agregar(mago);
            return mago;
        }

        [Fact]
        public void EjecutarTurno_AtaqueBasico_RegistraLasDosAccionesSinHabilidad()
        {
            var guerrero = GuerreroRegistrado();
            var mago = MagoRegistrado();
            var batalla = _service.CrearBatalla(guerrero, mago);

            _service.EjecutarTurno(batalla);

            Assert.Equal(2, _batallas.HistorialRegistrado.Count);
            Assert.All(_batallas.HistorialRegistrado, h => Assert.Null(h.HabilidadId));
            Assert.Equal((guerrero.Id, mago.Id), (_batallas.HistorialRegistrado[0].AtacanteId, _batallas.HistorialRegistrado[0].DefensorId));
            Assert.Equal((mago.Id, guerrero.Id), (_batallas.HistorialRegistrado[1].AtacanteId, _batallas.HistorialRegistrado[1].DefensorId));
        }

        [Fact]
        public void EjecutarTurno_ConHabilidad_RegistraLaHabilidadUsada()
        {
            var guerrero = GuerreroRegistrado(escudo: 20);
            var mago = MagoRegistrado();
            var batalla = _service.CrearBatalla(guerrero, mago);

            _service.EjecutarTurno(batalla, AccionCombate.Habilidad, AccionCombate.Atacar);

            Assert.Equal(7, _batallas.HistorialRegistrado[0].HabilidadId);
            Assert.Equal(34, _batallas.HistorialRegistrado[0].Dano); // Embestida: 18 + 20 - 4
            Assert.Null(_batallas.HistorialRegistrado[1].HabilidadId);
        }

        [Fact]
        public void EjecutarTurno_DanioAlEscudo_SeRegistraComoDanio()
        {
            var mago = MagoRegistrado();
            var guerrero = GuerreroRegistrado(escudo: 20);
            var batalla = _service.CrearBatalla(mago, guerrero);

            _service.EjecutarTurno(batalla);

            Assert.Equal(21, _batallas.HistorialRegistrado[0].Dano); // 14 + 15 - 8: 20 al escudo y 1 a la vida
        }

        [Fact]
        public void EjecutarTurno_DefensorDerrotado_RegistraUnaSolaAccion()
        {
            var guerrero = GuerreroRegistrado(escudo: 0);
            var mago = MagoRegistrado();
            var batalla = _service.CrearBatalla(guerrero, mago);
            mago.Vida = 10;

            bool sigue = _service.EjecutarTurno(batalla);

            Assert.False(sigue);
            Assert.Single(_batallas.HistorialRegistrado);
            Assert.Same(guerrero, batalla.Ganador);
        }

        [Fact]
        public void CrearBatalla_PersonajeDerrotadoEnOtraBatalla_EmpiezaRecuperado()
        {
            var guerrero = GuerreroRegistrado();
            var mago = MagoRegistrado();
            mago.Vida = 0;
            mago.Mana = 5;

            var batalla = _service.CrearBatalla(guerrero, mago);

            Assert.Equal(80, mago.Vida);
            Assert.Equal(60, mago.Mana);
            Assert.False(batalla.EsFinalizada);
        }

        [Fact]
        public void CrearBatalla_PersonajeSinRegistrar_LanzaExcepcion()
        {
            var guerrero = GuerreroRegistrado();
            var sinRegistrar = new Mago("Sin ID", 80, 14, 4, 60);

            Assert.Throws<InvalidOperationException>(() => _service.CrearBatalla(guerrero, sinRegistrar));
            Assert.Empty(_batallas.ObtenerTodos());
        }
    }
}
