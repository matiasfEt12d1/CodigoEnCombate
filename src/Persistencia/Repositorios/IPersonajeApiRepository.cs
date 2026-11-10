using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    // Personajes de la base combate_api.
    // Las lecturas usan por defecto la conexión del invitado; las escrituras, la del administrador.
    public interface IPersonajeApiRepository
    {
        IEnumerable<PersonajeRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.Invitado);
        PersonajeRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.Invitado);
        int Agregar(PersonajeRegistro personaje);
        bool ActualizarNombre(int id, string nombre);

        // Elimina solo si el personaje no participó en ningún combate.
        bool EliminarSiNoParticipo(int id);
    }
}
