using System;

namespace Persistencia.Entidades
{
    // Habilidad del Guerrero: descarga todo el escudo como daño adicional.
    public class Embestida : IHabilidad<Guerrero>
    {
        public string Nombre => "Embestida";

        public bool PuedeUsarse(Guerrero lanzador) => true;

        public void Aplicar(Guerrero lanzador, Personaje objetivo)
        {
            int danio = Math.Max(1, (lanzador.Ataque + lanzador.Escudo) - objetivo.Defensa);
            lanzador.Escudo = 0;
            objetivo.RecibirDanio(danio);
        }
    }
}
