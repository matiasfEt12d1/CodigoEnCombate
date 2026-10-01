using System;

namespace Persistencia.Entidades
{
    public class Mago : Personaje
    {
        private int _manaMax;
        private int _mana;

        public int ManaMax
        {
            get => _manaMax;
            set => _manaMax = value <= 0 
                ? throw new ArgumentException("El maná máximo debe ser mayor a cero.") 
                : value;
        }

        public int Mana
        {
            get => _mana;
            set
            {
                if (value < 0) _mana = 0;
                else if (value > ManaMax) _mana = ManaMax;
                else _mana = value;
            }
        }

        public Mago(string nombre, int vidaMax, int ataque, int defensa, int manaMax)
            : base(nombre, vidaMax, ataque, defensa)
        {
            ManaMax = manaMax;
            Mana = manaMax;
        }

        public override void Atacar(Personaje objetivo)
        {
            if (objetivo == null) throw new ArgumentNullException(nameof(objetivo));
            if (!EstaVivo) throw new InvalidOperationException("Un personaje derrotado no puede atacar.");

            int danioExtra = 0;
            if (Mana >= 10)
            {
                Mana -= 10;
                danioExtra = 15;
            }

            int danioTotal = Math.Max(1, (Ataque + danioExtra) - objetivo.Defensa);
            objetivo.RecibirDanio(danioTotal);
        }
    }
}