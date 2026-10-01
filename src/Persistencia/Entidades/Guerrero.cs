using System;

namespace Persistencia.Entidades
{
    public class Guerrero : Personaje
    {
        private int _escudo;

        public int Escudo
        {
            get => _escudo;
            set => _escudo = value < 0 
                ? throw new ArgumentException("El valor del escudo no puede ser negativo.") 
                : value;
        }

        public Guerrero(string nombre, int vidaMax, int ataque, int defensa, int escudo)
            : base(nombre, vidaMax, ataque, defensa)
        {
            Escudo = escudo;
        }

        public override void RecibirDanio(int cantidad)
        {
            if (cantidad < 0) return;

            if (Escudo > 0)
            {
                if (Escudo >= cantidad)
                {
                    Escudo -= cantidad;
                    return;
                }
                cantidad -= Escudo;
                Escudo = 0;
            }

            base.RecibirDanio(cantidad);
        }
    }
}