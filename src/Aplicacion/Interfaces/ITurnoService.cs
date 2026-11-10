using Aplicacion.Dtos;

namespace Aplicacion.Interfaces
{
    public interface ITurnoService
    {
        Resultado<IReadOnlyList<TurnoRespuesta>> ObtenerPorCombate(int combateId);
        Resultado<TurnoEjecutadoRespuesta> Ejecutar(int combateId, EjecutarTurnoSolicitud solicitud);
        Resultado<TurnoRespuesta> ActualizarNota(int combateId, int turnoId, ActualizarNotaSolicitud solicitud);
        Resultado EliminarUltimo(int combateId, int turnoId);
    }
}
