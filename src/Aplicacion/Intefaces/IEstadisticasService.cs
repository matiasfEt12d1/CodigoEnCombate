using System.Collections.Generic;
using Aplicacion;

namespace Aplicacion.Interfaces
{
    public interface IEstadisticasService
    {
        ResumenEstadisticas ObtenerResumenEstadisticas();
        IEnumerable<string> ObtenerRankingPersonajes();
    }
}