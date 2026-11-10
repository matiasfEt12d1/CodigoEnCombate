using Aplicacion.Dtos;

namespace Aplicacion.Interfaces
{
    public interface ICombateService
    {
        IReadOnlyList<CombateResumenRespuesta> ObtenerTodos();
        Resultado<CombateDetalleRespuesta> ObtenerPorId(int id);
        Resultado<CombateDetalleRespuesta> Crear(ParticipantesSolicitud solicitud);
        Resultado<CombateDetalleRespuesta> ActualizarParticipantes(int id, ParticipantesSolicitud solicitud);
        Resultado Eliminar(int id);
    }
}
