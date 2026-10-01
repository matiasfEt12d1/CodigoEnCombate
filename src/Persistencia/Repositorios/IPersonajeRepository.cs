using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IPersonajeRepository : IRepository<Personaje>
    {
        Personaje? ObtenerPorNombre(string nombre);
        IEnumerable<Personaje> ObtenerPorTipo(string tipo);
        bool AgregarHabilidad(int personajeId, Habilidad habilidad);
    }
}