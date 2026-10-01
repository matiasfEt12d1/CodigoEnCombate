using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IBatallaRepository : IRepository<Batalla>
    {
        IEnumerable<Batalla> ObtenerBatallasActivas();
    }
}