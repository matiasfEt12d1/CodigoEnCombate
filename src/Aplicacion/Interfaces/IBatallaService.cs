using System.Collections.Generic;
using Persistencia.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IBatallaService
    {
        Batalla CrearBatalla(Personaje combatiente1, Personaje combatiente2);
        Batalla CrearBatallaPorIds(int combatiente1Id, int combatiente2Id);
        bool EjecutarTurno(Batalla batalla);
        Personaje EjecutarCombateCompleto(Batalla batalla);
        Batalla? BuscarPorId(int id);
        IEnumerable<Batalla> ListarBatallas();
        IEnumerable<Batalla> ListarBatallasActivas();
        IEnumerable<Batalla> ObtenerHistorialCombates();
    }
}