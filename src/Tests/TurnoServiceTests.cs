using Aplicacion;
using Aplicacion.Dtos;
using Aplicacion.Servicios;
using Persistencia;
using Persistencia.Entidades;

namespace Tests
{
    // Ejecución, nota y borrado de turnos sobre repositorios en memoria (sin MySQL).
    // Combate 1: Thorin (Guerrero, id 1) contra Merlina (Mago, id 2); empieza Thorin.
    public class TurnoServiceTests
    {
        private const int CombateId = 1;
        private const int ThorinId = 1;
        private const int MerlinaId = 2;

        private readonly BaseEnMemoria _base = new();
        private readonly TurnoService _servicio;

        public TurnoServiceTests()
        {
            var thorin = CatalogoPersonajes.Nuevo("Thorin", "Guerrero");
            var merlina = CatalogoPersonajes.Nuevo("Merlina", "Mago");
            _base.PersonajesRepo.Agregar(thorin);
            _base.PersonajesRepo.Agregar(merlina);

            _base.CombatesRepo.Agregar(new CombateRegistro
            {
                Personaje1Id = thorin.Id,
                Personaje2Id = merlina.Id,
                Estado = EstadoCombate.EnCurso,
                TurnoPersonaje1 = true,
                NumeroTurno = 1,
                VidaPersonaje1 = thorin.VidaMax,
                VidaPersonaje2 = merlina.VidaMax,
                RecursoPersonaje1 = CatalogoPersonajes.RecursoInicial(thorin),
                RecursoPersonaje2 = CatalogoPersonajes.RecursoInicial(merlina)
            });

            _servicio = new TurnoService(_base.TurnosRepo, _base.CombatesRepo, _base.PersonajesRepo);
        }

        private CombateRegistro Combate() => _base.CombatesRepo.ObtenerPorId(CombateId)!;

        private TurnoEjecutadoRespuesta Ejecutar(int atacanteId, string accion)
        {
            var resultado = _servicio.Ejecutar(CombateId, new EjecutarTurnoSolicitud(atacanteId, accion));
            Assert.True(resultado.Exito, resultado.Mensaje);
            return resultado.Valor!;
        }

        private static void AssertMismoEstado(CombateRegistro esperado, CombateRegistro actual)
        {
            Assert.Equal(esperado.Estado, actual.Estado);
            Assert.Equal(esperado.TurnoPersonaje1, actual.TurnoPersonaje1);
            Assert.Equal(esperado.NumeroTurno, actual.NumeroTurno);
            Assert.Equal(esperado.VidaPersonaje1, actual.VidaPersonaje1);
            Assert.Equal(esperado.VidaPersonaje2, actual.VidaPersonaje2);
            Assert.Equal(esperado.RecursoPersonaje1, actual.RecursoPersonaje1);
            Assert.Equal(esperado.RecursoPersonaje2, actual.RecursoPersonaje2);
            Assert.Equal(esperado.GanadorId, actual.GanadorId);
        }

        // -----------------------------------------------------------------
        // Ejecutar turno
        // -----------------------------------------------------------------

        [Fact]
        public void Ejecutar_TurnoValido_GuardaElTurnoYLeTocaAlOtro()
        {
            var respuesta = Ejecutar(ThorinId, "atacar");

            Assert.Equal(1, respuesta.Turno.Numero);
            Assert.Equal(ThorinId, respuesta.Turno.AtacanteId);
            Assert.Equal(MerlinaId, respuesta.Turno.DefensorId);
            Assert.Equal(14, respuesta.Turno.Danio);
            Assert.Equal(66, respuesta.Combate.Personaje2.Vida);
            Assert.Equal(MerlinaId, respuesta.Combate.TurnoDeId);
            Assert.Equal(2, respuesta.Combate.NumeroTurno);
            Assert.Single(_base.Turnos);
        }

        [Fact]
        public void Ejecutar_TurnosAlternados_CadaUnoActuaUnaVez()
        {
            Ejecutar(ThorinId, "atacar");
            Ejecutar(MerlinaId, "habilidad");
            var respuesta = Ejecutar(ThorinId, "atacar");

            Assert.Equal(MerlinaId, respuesta.Combate.TurnoDeId);
            Assert.Equal(new[] { ThorinId, MerlinaId, ThorinId }, _base.Turnos.Select(t => t.AtacanteId));
        }

        [Fact]
        public void Ejecutar_FueraDeTurno_DevuelveErrorDeValidacionSinGuardar()
        {
            var antes = Combate();

            var resultado = _servicio.Ejecutar(CombateId, new EjecutarTurnoSolicitud(MerlinaId, "atacar"));

            Assert.Equal(TipoError.Validacion, resultado.Error);
            Assert.Empty(_base.Turnos);
            AssertMismoEstado(antes, Combate());
        }

        [Theory]
        [InlineData("defender")]
        [InlineData("")]
        [InlineData(null)]
        public void Ejecutar_AccionInvalida_DevuelveErrorDeValidacion(string? accion)
        {
            var resultado = _servicio.Ejecutar(CombateId, new EjecutarTurnoSolicitud(ThorinId, accion));

            Assert.Equal(TipoError.Validacion, resultado.Error);
            Assert.Empty(_base.Turnos);
        }

        [Fact]
        public void Ejecutar_AtacanteQueNoParticipa_DevuelveNoEncontrado()
        {
            var resultado = _servicio.Ejecutar(CombateId, new EjecutarTurnoSolicitud(99, "atacar"));

            Assert.Equal(TipoError.NoEncontrado, resultado.Error);
        }

        [Fact]
        public void Ejecutar_CombateInexistente_DevuelveNoEncontrado()
        {
            var resultado = _servicio.Ejecutar(99, new EjecutarTurnoSolicitud(ThorinId, "atacar"));

            Assert.Equal(TipoError.NoEncontrado, resultado.Error);
        }

        [Fact]
        public void Ejecutar_GolpeFinal_FinalizaElCombateConGanador()
        {
            var combate = Combate();
            combate.VidaPersonaje2 = 10;
            _base.CombatesRepo.Reemplazar(combate);

            var respuesta = Ejecutar(ThorinId, "atacar");

            Assert.Equal(EstadoCombate.Finalizado, respuesta.Combate.Estado);
            Assert.Equal(ThorinId, respuesta.Combate.GanadorId);
            Assert.Equal("Thorin", respuesta.Combate.GanadorNombre);
            Assert.Null(respuesta.Combate.TurnoDeId);
            Assert.Equal(0, respuesta.Combate.Personaje2.Vida);
        }

        [Fact]
        public void Ejecutar_CombateFinalizado_DevuelveErrorDeValidacion()
        {
            var combate = Combate();
            combate.VidaPersonaje2 = 10;
            _base.CombatesRepo.Reemplazar(combate);
            Ejecutar(ThorinId, "atacar");

            var resultado = _servicio.Ejecutar(CombateId, new EjecutarTurnoSolicitud(MerlinaId, "atacar"));

            Assert.Equal(TipoError.Validacion, resultado.Error);
            Assert.Single(_base.Turnos);
        }

        [Fact]
        public void Ejecutar_DanioAlEscudo_CuentaComoDanioDelTurno()
        {
            Ejecutar(ThorinId, "atacar");

            var respuesta = Ejecutar(MerlinaId, "atacar"); // 14 + 15 - 8 = 21: 20 al escudo y 1 a la vida

            Assert.Equal(21, respuesta.Turno.Danio);
            Assert.Equal(0, respuesta.Combate.Personaje1.Recurso);
            Assert.Equal(119, respuesta.Combate.Personaje1.Vida);
        }

        // -----------------------------------------------------------------
        // Nota del turno
        // -----------------------------------------------------------------

        [Fact]
        public void ActualizarNota_NoAlteraElDanioNiElCombate()
        {
            var turno = Ejecutar(ThorinId, "habilidad").Turno;
            var combateAntes = Combate();

            var resultado = _servicio.ActualizarNota(CombateId, turno.Id, new ActualizarNotaSolicitud("Embestida inicial"));

            Assert.True(resultado.Exito, resultado.Mensaje);
            Assert.Equal("Embestida inicial", resultado.Valor!.Nota);
            Assert.Equal(turno.Danio, resultado.Valor.Danio);
            Assert.Equal(turno.AtacanteId, resultado.Valor.AtacanteId);
            Assert.Equal(turno.Accion, resultado.Valor.Accion);
            AssertMismoEstado(combateAntes, Combate());
        }

        [Fact]
        public void ActualizarNota_DemasiadoLarga_DevuelveErrorDeValidacion()
        {
            var turno = Ejecutar(ThorinId, "atacar").Turno;
            var nota = new string('x', TurnoService.LargoMaximoNota + 1);

            var resultado = _servicio.ActualizarNota(CombateId, turno.Id, new ActualizarNotaSolicitud(nota));

            Assert.Equal(TipoError.Validacion, resultado.Error);
            Assert.Null(_base.Turnos.Single().Nota);
        }

        [Fact]
        public void ActualizarNota_TurnoInexistente_DevuelveNoEncontrado()
        {
            var resultado = _servicio.ActualizarNota(CombateId, 99, new ActualizarNotaSolicitud("Nota"));

            Assert.Equal(TipoError.NoEncontrado, resultado.Error);
        }

        // -----------------------------------------------------------------
        // Borrar el último turno
        // -----------------------------------------------------------------

        [Fact]
        public void EliminarUltimo_RestauraElEstadoPrevioDelCombate()
        {
            Ejecutar(ThorinId, "atacar");
            var estadoPrevio = Combate();
            var ultimo = Ejecutar(MerlinaId, "habilidad").Turno;

            var resultado = _servicio.EliminarUltimo(CombateId, ultimo.Id);

            Assert.True(resultado.Exito, resultado.Mensaje);
            Assert.Single(_base.Turnos);
            AssertMismoEstado(estadoPrevio, Combate());
        }

        [Fact]
        public void EliminarUltimo_TurnoQueFinalizoElCombate_VuelveAEnCursoSinGanador()
        {
            var combate = Combate();
            combate.VidaPersonaje2 = 10;
            _base.CombatesRepo.Reemplazar(combate);
            var estadoPrevio = Combate();
            var turno = Ejecutar(ThorinId, "atacar").Turno;

            var resultado = _servicio.EliminarUltimo(CombateId, turno.Id);

            Assert.True(resultado.Exito, resultado.Mensaje);
            Assert.Empty(_base.Turnos);
            AssertMismoEstado(estadoPrevio, Combate());
            Assert.Equal(EstadoCombate.EnCurso, Combate().Estado);
            Assert.Null(Combate().GanadorId);
        }

        [Fact]
        public void EliminarUltimo_DespuesDeRestaurar_ElMismoAtacantePuedeVolverAActuar()
        {
            Ejecutar(ThorinId, "atacar");
            var ultimo = Ejecutar(MerlinaId, "atacar").Turno;
            _servicio.EliminarUltimo(CombateId, ultimo.Id);

            var repetido = Ejecutar(MerlinaId, "atacar").Turno;

            Assert.Equal(ultimo.Numero, repetido.Numero);
            Assert.Equal(ultimo.Danio, repetido.Danio);
        }

        [Fact]
        public void EliminarUltimo_TurnoQueNoEsElUltimo_DevuelveConflictoSinCambios()
        {
            var primero = Ejecutar(ThorinId, "atacar").Turno;
            Ejecutar(MerlinaId, "atacar");
            var antes = Combate();

            var resultado = _servicio.EliminarUltimo(CombateId, primero.Id);

            Assert.Equal(TipoError.Conflicto, resultado.Error);
            Assert.Equal(2, _base.Turnos.Count);
            AssertMismoEstado(antes, Combate());
        }

        [Fact]
        public void EliminarUltimo_TurnoInexistente_DevuelveNoEncontrado()
        {
            var resultado = _servicio.EliminarUltimo(CombateId, 99);

            Assert.Equal(TipoError.NoEncontrado, resultado.Error);
        }
    }
}
