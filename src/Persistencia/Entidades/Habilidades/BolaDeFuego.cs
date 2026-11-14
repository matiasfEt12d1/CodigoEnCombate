namespace Persistencia.Entidades
{
    // Habilidad del Mago: consume maná y hace el doble de ataque ignorando la defensa.
    public class BolaDeFuego : IHabilidad<Mago>
    {
        public string Nombre => "Bola de fuego";

        public bool PuedeUsarse(Mago lanzador) => lanzador.Mana >= Mago.CostoBolaDeFuego;

        public void Aplicar(Mago lanzador, Personaje objetivo)
        {
            lanzador.Mana -= Mago.CostoBolaDeFuego;
            objetivo.RecibirDanio(lanzador.Ataque * 2);
        }
    }
}
