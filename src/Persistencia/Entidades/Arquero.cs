using System;

namespace Persistencia.Entidades
{
    public class Arquero : Personaje
    {
        public const int FlechasDisparoDoble = 2;

        private int _cantidadFlechas;

        public int CantidadFlechas
        {
            get => _cantidadFlechas;
            set => _cantidadFlechas = value < 0 
                ? throw new ArgumentException("La cantidad de flechas no puede ser negativa.") 
                : value;
        }

        public Arquero(string nombre, int vidaMax, int ataque, int defensa, int cantidadFlechas)
            : base(nombre, vidaMax, ataque, defensa)
        {
            CantidadFlechas = cantidadFlechas;
        }

        public override void Atacar(Personaje objetivo)
        {
            if (objetivo == null) throw new ArgumentNullException(nameof(objetivo));
            if (!EstaVivo) throw new InvalidOperationException("Un personaje derrotado no puede atacar.");

            int ataqueEfectivo = Ataque;
            if (CantidadFlechas > 0)
            {
                CantidadFlechas--;
            }
            else
            {
                ataqueEfectivo = Math.Max(1, Ataque / 2);
            }

            int danio = Math.Max(1, ataqueEfectivo - objetivo.Defensa);
            objetivo.RecibirDanio(danio);
        }

        // Disparo doble: consume dos flechas y la defensa se aplica a cada disparo.
        public override void UsarHabilidad(Personaje objetivo)
        {
            ValidarPuedeActuar(objetivo);
            if (CantidadFlechas < FlechasDisparoDoble)
                throw new InvalidOperationException("No hay flechas suficientes para Disparo doble.");

            CantidadFlechas -= FlechasDisparoDoble;
            int danioPorFlecha = Math.Max(1, Ataque - objetivo.Defensa);
            objetivo.RecibirDanio(danioPorFlecha * FlechasDisparoDoble);
        }
    }
}