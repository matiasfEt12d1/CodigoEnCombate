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

        public override int VidaEfectiva => Vida + Escudo;

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

        // Embestida: descarga todo el escudo como daño adicional.
        public override void UsarHabilidad(Personaje objetivo)
        {
            ValidarPuedeActuar(objetivo);

            int danio = Math.Max(1, (Ataque + Escudo) - objetivo.Defensa);
            Escudo = 0;
            objetivo.RecibirDanio(danio);
        }
    }
}