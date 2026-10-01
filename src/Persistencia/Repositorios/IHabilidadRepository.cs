using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IHabilidadRepository : IRepository<Habilidad>
    {
        IEnumerable<Habilidad> ObtenerPorPersonajeId(int personajeId);
    }
}