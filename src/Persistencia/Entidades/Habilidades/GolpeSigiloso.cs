using System;

namespace Persistencia.Entidades
{
    // Habilidad del Asesino: crítico garantizado, sin depender de la probabilidad.
    public class GolpeSigiloso : IHabilidad<Asesino>
    {
        public string Nombre => "Golpe sigiloso";

        public bool PuedeUsarse(Asesino lanzador) => true;

        public void Aplicar(Asesino lanzador, Personaje objetivo)
        {
            int danio = Math.Max(1, (lanzador.Ataque * 2) - objetivo.Defensa);
            objetivo.RecibirDanio(danio);
        }
    }
}
