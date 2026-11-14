using System;

namespace Persistencia.Entidades
{
    // Habilidad del Arquero: consume dos flechas y la defensa se aplica a cada disparo.
    public class DisparoDoble : IHabilidad<Arquero>
    {
        public string Nombre => "Disparo doble";

        public bool PuedeUsarse(Arquero lanzador) => lanzador.CantidadFlechas >= Arquero.FlechasDisparoDoble;

        public void Aplicar(Arquero lanzador, Personaje objetivo)
        {
            lanzador.CantidadFlechas -= Arquero.FlechasDisparoDoble;
            int danioPorFlecha = Math.Max(1, lanzador.Ataque - objetivo.Defensa);
            objetivo.RecibirDanio(danioPorFlecha * Arquero.FlechasDisparoDoble);
        }
    }
}
