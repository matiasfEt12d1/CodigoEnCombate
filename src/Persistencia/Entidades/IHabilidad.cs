namespace Persistencia.Entidades
{
    // Comportamiento de una habilidad especial. Cada clase de personaje se compone de la suya
    // y UsarHabilidad le delega la acción.
    // No confundir con Habilidad, que es el registro guardado en la base para el historial y las estadísticas.
    public interface IHabilidad<in TLanzador> where TLanzador : Personaje
    {
        string Nombre { get; }

        // Si el lanzador tiene los recursos que la habilidad consume.
        bool PuedeUsarse(TLanzador lanzador);

        // Consume los recursos y aplica el daño al objetivo.
        void Aplicar(TLanzador lanzador, Personaje objetivo);
    }
}
