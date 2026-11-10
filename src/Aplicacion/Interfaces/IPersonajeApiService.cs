using Aplicacion.Dtos;

namespace Aplicacion.Interfaces
{
    public interface IPersonajeApiService
    {
        IReadOnlyList<PersonajeRespuesta> ObtenerTodos();
        Resultado<PersonajeRespuesta> ObtenerPorId(int id);
        Resultado<IReadOnlyList<CombateResumenRespuesta>> ObtenerCombates(int id);
        Resultado<PersonajeRespuesta> Crear(CrearPersonajeSolicitud solicitud);
        Resultado<PersonajeRespuesta> ActualizarNombre(int id, ActualizarPersonajeSolicitud solicitud);
        Resultado Eliminar(int id);
    }
}
