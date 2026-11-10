using System.Collections.Generic;
using Persistencia.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IPersonajeService
    {
        int RegistrarPersonaje(Personaje personaje);
        Personaje? ObtenerPorId(int id);
        Personaje? ObtenerPorNombre(string nombre);
        IEnumerable<Personaje> ObtenerTodos();
        IEnumerable<Personaje> ObtenerPorTipo(string tipo);
        bool AgregarHabilidadAPersonaje(int personajeId, Habilidad habilidad);
    }
}