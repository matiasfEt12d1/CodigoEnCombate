using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IBatallaRepository : IRepository<Batalla>
    {
        IEnumerable<Batalla> ObtenerBatallasActivas();
        void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, string? habilidadUsada, int danoCausado);
    }
}