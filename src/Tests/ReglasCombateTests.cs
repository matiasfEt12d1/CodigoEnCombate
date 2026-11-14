using Persistencia;
using Persistencia.Entidades;

namespace Tests
{
    // Reglas del dominio (Persistencia/Entidades) sin base de datos.
    // Estadísticas iguales a las del catálogo: Guerrero 120/18/8 escudo 20, Mago 80/14/4 maná 60,
    // Arquero 90/16/5 flechas 12, Asesino 85/20/4.
    public class ReglasCombateTests
    {
        private static Guerrero NuevoGuerrero(int escudo = 20) => new("Thorin", 120, 18, 8, escudo);
        private static Mago NuevoMago() => new("Merlina", 80, 14, 4, 60);
        private static Arquero NuevoArquero(int flechas = 12) => new("Legolia", 90, 16, 5, flechas);
        // 0.0 = nunca crítico, 1.0 = siempre crítico.
        private static Asesino NuevoAsesino(double critico = 0.0) => new("Sombra", 85, 20, 4, critico);

        public static TheoryData<string> Tipos => new() { "Guerrero", "Mago", "Arquero", "Asesino" };

        private static Personaje CrearDelCatalogo(string tipo, int id, int? vida = null)
        {
            var registro = CatalogoPersonajes.Nuevo($"{tipo} {id}", tipo);
            registro.Id = id;
            return CatalogoPersonajes.CrearEnCombate(registro, vida ?? registro.VidaMax, CatalogoPersonajes.RecursoInicial(registro));
        }

        // -----------------------------------------------------------------
        // Daño
        // -----------------------------------------------------------------

        [Fact]
        public void Atacar_ObjetivoSinEscudo_RestaAtaqueMenosDefensa()
        {
            var guerrero = NuevoGuerrero();
            var asesino = NuevoAsesino();

            guerrero.Atacar(asesino);

            Assert.Equal(85 - (18 - 4), asesino.Vida);
        }

        [Fact]
        public void Atacar_DefensaMayorQueAtaque_HaceDanioMinimoDeUno()
        {
            var debil = new Guerrero("Débil", 50, 3, 0, 0);
            var objetivo = NuevoGuerrero(escudo: 0);

            debil.Atacar(objetivo);

            Assert.Equal(119, objetivo.Vida);
        }

        [Fact]
        public void Atacar_GuerreroConEscudo_ElEscudoAbsorbeAntesQueLaVida()
        {
            var asesino = NuevoAsesino();
            var guerrero = NuevoGuerrero(escudo: 20);

            asesino.Atacar(guerrero); // 20 - 8 = 12

            Assert.Equal(8, guerrero.Escudo);
            Assert.Equal(120, guerrero.Vida);
        }

        [Fact]
        public void Atacar_DanioMayorQueElEscudo_RompeEscudoYRestaVida()
        {
            var asesino = NuevoAsesino(critico: 1.0);
            var guerrero = NuevoGuerrero(escudo: 10);

            asesino.Atacar(guerrero); // 40 - 8 = 32: 10 al escudo y 22 a la vida

            Assert.Equal(0, guerrero.Escudo);
            Assert.Equal(98, guerrero.Vida);
        }

        [Fact]
        public void Atacar_MagoConMana_SumaDanioExtraYConsumeMana()
        {
            var mago = NuevoMago();
            var arquero = NuevoArquero();

            mago.Atacar(arquero); // 14 + 15 - 5 = 24

            Assert.Equal(66, arquero.Vida);
            Assert.Equal(50, mago.Mana);
        }

        [Fact]
        public void Atacar_MagoSinMana_HaceDanioBase()
        {
            var mago = NuevoMago();
            mago.Mana = 5;
            var arquero = NuevoArquero();

            mago.Atacar(arquero); // 14 - 5 = 9

            Assert.Equal(81, arquero.Vida);
            Assert.Equal(5, mago.Mana);
        }

        [Fact]
        public void Atacar_ArqueroConFlechas_GastaUnaFlecha()
        {
            var arquero = NuevoArquero();
            var mago = NuevoMago();

            arquero.Atacar(mago); // 16 - 4 = 12

            Assert.Equal(68, mago.Vida);
            Assert.Equal(11, arquero.CantidadFlechas);
        }

        [Fact]
        public void Atacar_ArqueroSinFlechas_AtacaConLaMitad()
        {
            var arquero = NuevoArquero(flechas: 0);
            var mago = NuevoMago();

            arquero.Atacar(mago); // 16 / 2 - 4 = 4

            Assert.Equal(76, mago.Vida);
        }

        [Fact]
        public void Atacar_AsesinoConCritico_DuplicaElAtaque()
        {
            var asesino = NuevoAsesino(critico: 1.0);
            var guerrero = NuevoGuerrero(escudo: 0);

            asesino.Atacar(guerrero); // 40 - 8 = 32

            Assert.Equal(88, guerrero.Vida);
        }

        // -----------------------------------------------------------------
        // Habilidades
        // -----------------------------------------------------------------

        [Fact]
        public void Habilidad_GuerreroEmbestida_SumaElEscudoAlAtaqueYLoGasta()
        {
            var guerrero = NuevoGuerrero(escudo: 20);
            var mago = NuevoMago();

            guerrero.UsarHabilidad(mago); // 18 + 20 - 4 = 34

            Assert.Equal(46, mago.Vida);
            Assert.Equal(0, guerrero.Escudo);
        }

        [Fact]
        public void Habilidad_MagoBolaDeFuego_DobleAtaqueIgnorandoDefensa()
        {
            var mago = NuevoMago();
            var guerrero = NuevoGuerrero(escudo: 0);

            mago.UsarHabilidad(guerrero); // 14 * 2 = 28

            Assert.Equal(92, guerrero.Vida);
            Assert.Equal(60 - Mago.CostoBolaDeFuego, mago.Mana);
        }

        [Fact]
        public void Habilidad_MagoSinManaSuficiente_LanzaExcepcionSinHacerDanio()
        {
            var mago = NuevoMago();
            mago.Mana = Mago.CostoBolaDeFuego - 1;
            var guerrero = NuevoGuerrero(escudo: 0);

            Assert.Throws<InvalidOperationException>(() => mago.UsarHabilidad(guerrero));
            Assert.Equal(120, guerrero.Vida);
            Assert.Equal(Mago.CostoBolaDeFuego - 1, mago.Mana);
        }

        [Fact]
        public void Habilidad_ArqueroDisparoDoble_DosDisparosConDefensaCadaUno()
        {
            var arquero = NuevoArquero();
            var asesino = NuevoAsesino();

            arquero.UsarHabilidad(asesino); // (16 - 4) * 2 = 24

            Assert.Equal(61, asesino.Vida);
            Assert.Equal(10, arquero.CantidadFlechas);
        }

        [Fact]
        public void Habilidad_ArqueroConUnaFlecha_LanzaExcepcionSinHacerDanio()
        {
            var arquero = NuevoArquero(flechas: 1);
            var asesino = NuevoAsesino();

            Assert.Throws<InvalidOperationException>(() => arquero.UsarHabilidad(asesino));
            Assert.Equal(85, asesino.Vida);
            Assert.Equal(1, arquero.CantidadFlechas);
        }

        [Fact]
        public void Habilidad_AsesinoGolpeSigiloso_CriticoGarantizado()
        {
            var asesino = NuevoAsesino(critico: 0.0);
            var arquero = NuevoArquero();

            asesino.UsarHabilidad(arquero); // 20 * 2 - 5 = 35

            Assert.Equal(55, arquero.Vida);
        }

        // -----------------------------------------------------------------
        // Turnos alternados
        // -----------------------------------------------------------------

        [Fact]
        public void Batalla_Nueva_EmpiezaElCombatiente1()
        {
            var guerrero = NuevoGuerrero();
            var mago = NuevoMago();

            var batalla = new Batalla(guerrero, mago);

            Assert.Same(guerrero, batalla.AtacanteActual);
            Assert.Same(mago, batalla.DefensorActual);
        }

        [Fact]
        public void Batalla_DespuesDeCadaAccion_PasaElTurnoAlOtro()
        {
            var guerrero = NuevoGuerrero();
            var mago = NuevoMago();
            var batalla = new Batalla(guerrero, mago);

            batalla.EjecutarAtaque(guerrero);
            Assert.Same(mago, batalla.AtacanteActual);
            Assert.Same(guerrero, batalla.DefensorActual);

            batalla.EjecutarHabilidad(mago);
            Assert.Same(guerrero, batalla.AtacanteActual);

            batalla.EjecutarAtaque(guerrero);
            Assert.Same(mago, batalla.AtacanteActual);
        }

        [Fact]
        public void Batalla_ElDefensorEsSiempreElOtroParticipante()
        {
            var guerrero = NuevoGuerrero(escudo: 0);
            var mago = NuevoMago();
            var batalla = new Batalla(guerrero, mago);

            batalla.EjecutarAtaque(guerrero);
            Assert.Equal(80 - 14, mago.Vida);

            batalla.EjecutarAtaque(mago);
            Assert.Equal(120 - 21, guerrero.Vida);
        }

        [Fact]
        public void Batalla_ActuarFueraDeTurno_LanzaExcepcionSinCambios()
        {
            var guerrero = NuevoGuerrero();
            var mago = NuevoMago();
            var batalla = new Batalla(guerrero, mago);

            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarAtaque(mago));
            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarHabilidad(mago));

            Assert.Equal(120, guerrero.Vida);
            Assert.Equal(20, guerrero.Escudo);
            Assert.Equal(60, mago.Mana);
            Assert.Same(guerrero, batalla.AtacanteActual);
        }

        [Fact]
        public void Batalla_HabilidadFallida_NoPasaElTurno()
        {
            var arquero = NuevoArquero(flechas: 1);
            var asesino = NuevoAsesino();
            var batalla = new Batalla(arquero, asesino);

            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarHabilidad(arquero));

            Assert.Same(arquero, batalla.AtacanteActual);
        }

        // -----------------------------------------------------------------
        // Personaje derrotado
        // -----------------------------------------------------------------

        [Theory]
        [MemberData(nameof(Tipos))]
        public void Derrotado_NoPuedeAtacar(string tipo)
        {
            var derrotado = CrearDelCatalogo(tipo, 1, vida: 0);
            var objetivo = NuevoGuerrero(escudo: 0);

            Assert.False(derrotado.EstaVivo);
            Assert.Throws<InvalidOperationException>(() => derrotado.Atacar(objetivo));
            Assert.Equal(120, objetivo.Vida);
        }

        [Theory]
        [MemberData(nameof(Tipos))]
        public void Derrotado_NoPuedeUsarHabilidad(string tipo)
        {
            var derrotado = CrearDelCatalogo(tipo, 1, vida: 0);
            var objetivo = NuevoGuerrero(escudo: 0);

            Assert.Throws<InvalidOperationException>(() => derrotado.UsarHabilidad(objetivo));
            Assert.Equal(120, objetivo.Vida);
        }

        [Fact]
        public void Derrotado_LaVidaNoBajaDeCero()
        {
            var mago = NuevoMago();

            mago.RecibirDanio(500);

            Assert.Equal(0, mago.Vida);
            Assert.False(mago.EstaVivo);
        }

        // -----------------------------------------------------------------
        // Finalización del combate
        // -----------------------------------------------------------------

        [Fact]
        public void Batalla_EnCurso_NoTieneGanador()
        {
            var batalla = new Batalla(NuevoGuerrero(), NuevoMago());

            Assert.False(batalla.EsFinalizada);
            Assert.Null(batalla.Ganador);
        }

        [Fact]
        public void Batalla_DefensorLlegaACero_FinalizaYGanaElAtacante()
        {
            var guerrero = NuevoGuerrero(escudo: 0);
            var mago = NuevoMago();
            mago.Vida = 10;
            var batalla = new Batalla(guerrero, mago);

            batalla.EjecutarAtaque(guerrero);

            Assert.True(batalla.EsFinalizada);
            Assert.Same(guerrero, batalla.Ganador);
            Assert.Equal(0, mago.Vida);
        }

        [Fact]
        public void Batalla_Finalizada_NoAceptaMasAcciones()
        {
            var guerrero = NuevoGuerrero(escudo: 0);
            var mago = NuevoMago();
            mago.Vida = 10;
            var batalla = new Batalla(guerrero, mago);
            batalla.EjecutarAtaque(guerrero);

            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarAtaque(mago));
            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarAtaque(guerrero));
            Assert.Throws<InvalidOperationException>(() => batalla.EjecutarHabilidad(guerrero));
        }

        [Theory]
        [MemberData(nameof(Tipos))]
        public void Batalla_CombateCompleto_TerminaConUnGanadorVivo(string tipoRival)
        {
            var guerrero = CrearDelCatalogo("Guerrero", 1);
            var rival = CrearDelCatalogo(tipoRival, 2);
            var batalla = new Batalla(guerrero, rival);

            for (int i = 0; i < 200 && !batalla.EsFinalizada; i++)
                batalla.EjecutarAtaque(batalla.AtacanteActual);

            Assert.True(batalla.EsFinalizada);
            Assert.NotNull(batalla.Ganador);
            Assert.True(batalla.Ganador!.EstaVivo);
            Assert.False(batalla.DefensorActual.EstaVivo);
        }
    }
}
