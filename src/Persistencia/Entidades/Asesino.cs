using System;

namespace Persistencia.Entidades
{
    public class Asesino : Personaje
    {
        private double _probabilidadCritico;

        public double ProbabilidadCritico
        {
            get => _probabilidadCritico;
            set => _probabilidadCritico = (value < 0.0 || value > 1.0) 
                ? throw new ArgumentOutOfRangeException(nameof(value), "La probabilidad de crítico debe estar entre 0.0 y 1.0.") 
                : value;
        }

        public Asesino(string nombre, int vidaMax, int ataque, int defensa, double probabilidadCritico)
            : base(nombre, vidaMax, ataque, defensa)
        {
            ProbabilidadCritico = probabilidadCritico;
        }

        public override void Atacar(Personaje objetivo)
        {
            if (objetivo == null) throw new ArgumentNullException(nameof(objetivo));
            if (!EstaVivo) throw new InvalidOperationException("Un personaje derrotado no puede atacar.");

            bool esCritico = Random.Shared.NextDouble() <= ProbabilidadCritico;
            int ataqueEfectivo = esCritico ? Ataque * 2 : Ataque;
            int danio = Math.Max(1, ataqueEfectivo - objetivo.Defensa);
            objetivo.RecibirDanio(danio);
        }
    }
}