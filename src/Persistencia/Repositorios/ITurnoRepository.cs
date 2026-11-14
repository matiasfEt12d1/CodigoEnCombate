using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    // Turnos de la base combate_api.
    // Las lecturas usan por defecto la conexión del invitado; las escrituras, la del administrador.
    public interface ITurnoRepository
    {
        IEnumerable<TurnoRegistro> ObtenerPorCombate(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado);
        TurnoRegistro? ObtenerPorId(int combateId, int turnoId, TipoConexion conexion = TipoConexion.ApiInvitado);
        TurnoRegistro? ObtenerUltimo(int combateId, TipoConexion conexion = TipoConexion.ApiInvitado);

        // En una transacción: inserta el turno y actualiza el combate.
        // Devuelve false (sin cambios) si el combate ya no está en el turno esperado.
        bool Registrar(TurnoRegistro turno, CombateRegistro combateActualizado, int numeroTurnoEsperado);

        bool ActualizarNota(int combateId, int turnoId, string? nota);

        // En una transacción: borra el último turno y restaura el combate.
        // Devuelve false (sin cambios) si el turno dejó de ser el último.
        bool EliminarUltimoYRestaurar(TurnoRegistro turno, CombateRegistro combateRestaurado);
    }
}
