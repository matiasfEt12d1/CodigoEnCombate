using Persistencia.Entidades;

namespace Tests
{
    // Habilidades por interfaz (IHabilidad) y operadores == / != de Personaje, sin base de datos.
    public class HabilidadesYOperadoresTests
    {
        // -----------------------------------------------------------------
        // IHabilidad
        // -----------------------------------------------------------------

        [Fact]
        public void Embestida_SumaElEscudoAlAtaqueYLoGasta()
        {
            IHabilidad<Guerrero> embestida = new Embestida();
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 20);
            var mago = new Mago("Merlina", 80, 14, 4, 60);

            embestida.Aplicar(guerrero, mago); // 18 + 20 - 4 = 34

            Assert.Equal(46, mago.Vida);
            Assert.Equal(0, guerrero.Escudo);
        }

        [Theory]
        [InlineData(60, true)]
        [InlineData(25, true)]
        [InlineData(24, false)]
        public void BolaDeFuego_PuedeUsarseSoloConManaSuficiente(int mana, bool esperado)
        {
            IHabilidad<Mago> bola = new BolaDeFuego();
            var mago = new Mago("Merlina", 80, 14, 4, 60) { Mana = mana };

            Assert.Equal(esperado, bola.PuedeUsarse(mago));
            Assert.Equal(esperado, mago.PuedeUsarHabilidad);
        }

        [Theory]
        [InlineData(12, true)]
        [InlineData(2, true)]
        [InlineData(1, false)]
        public void DisparoDoble_PuedeUsarseSoloConDosFlechas(int flechas, bool esperado)
        {
            IHabilidad<Arquero> disparo = new DisparoDoble();
            var arquero = new Arquero("Legolia", 90, 16, 5, flechas);

            Assert.Equal(esperado, disparo.PuedeUsarse(arquero));
            Assert.Equal(esperado, arquero.PuedeUsarHabilidad);
        }

        [Fact]
        public void GolpeSigiloso_HaceCriticoGarantizado()
        {
            IHabilidad<Asesino> golpe = new GolpeSigiloso();
            var asesino = new Asesino("Sombra", 85, 20, 4, 0.0);
            var arquero = new Arquero("Legolia", 90, 16, 5, 12);

            golpe.Aplicar(asesino, arquero); // 20 * 2 - 5 = 35

            Assert.Equal(55, arquero.Vida);
        }

        [Fact]
        public void PuedeUsarHabilidad_PersonajeDerrotado_EsFalso()
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 20) { Vida = 0 };
            var asesino = new Asesino("Sombra", 85, 20, 4, 0.0) { Vida = 0 };

            Assert.False(guerrero.PuedeUsarHabilidad);
            Assert.False(asesino.PuedeUsarHabilidad);
        }

        [Fact]
        public void UsarHabilidad_SinRecursos_LanzaExcepcionConElNombreDeLaHabilidad()
        {
            var mago = new Mago("Merlina", 80, 14, 4, 60) { Mana = 0 };
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 0);

            var ex = Assert.Throws<InvalidOperationException>(() => mago.UsarHabilidad(guerrero));

            Assert.Contains("Bola de fuego", ex.Message);
            Assert.Equal(120, guerrero.Vida);
        }

        // -----------------------------------------------------------------
        // Operadores == y !=
        // -----------------------------------------------------------------

        [Fact]
        public void OperadorIgual_MismoObjeto_EsVerdadero()
        {
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 20);
            var mismo = guerrero;

            Assert.True(guerrero == mismo);
            Assert.False(guerrero != mismo);
        }

        [Fact]
        public void OperadorIgual_DosInstanciasConElMismoId_EsVerdadero()
        {
            var original = new Guerrero("Thorin", 120, 18, 8, 20) { Id = 3 };
            var copia = new Guerrero("Thorin", 120, 18, 8, 20) { Id = 3 };

            Assert.True(original == copia);
            Assert.True(original.Equals(copia));
            Assert.Equal(original.GetHashCode(), copia.GetHashCode());
        }

        [Fact]
        public void OperadorIgual_IdsDistintos_EsFalso()
        {
            var thorin = new Guerrero("Thorin", 120, 18, 8, 20) { Id = 1 };
            var merlina = new Mago("Merlina", 80, 14, 4, 60) { Id = 2 };

            Assert.True(thorin != merlina);
        }

        [Fact]
        public void OperadorIgual_SinGuardarYMismosDatos_EsFalso()
        {
            var uno = new Guerrero("Thorin", 120, 18, 8, 20);
            var otro = new Guerrero("Thorin", 120, 18, 8, 20);

            Assert.False(uno == otro);
        }

        [Fact]
        public void OperadorIgual_ConNull_FuncionaSinExcepciones()
        {
            Personaje? nadie = null;
            var guerrero = new Guerrero("Thorin", 120, 18, 8, 20);

            Assert.False(guerrero == nadie);
            Assert.True(nadie == null);
        }
    }
}
