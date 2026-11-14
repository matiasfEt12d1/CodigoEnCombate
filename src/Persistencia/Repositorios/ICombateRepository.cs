using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    // Combates de la base combate_api.
    // Las lecturas usan por defecto la conexión del invitado; las escrituras, la del administrador.
    public interface ICombateRepository
    {
        IEnumerable<CombateRegistro> ObtenerTodos(TipoConexion conexion = TipoConexion.ApiInvitado);
        IEnumerable<CombateRegistro> ObtenerPorPersonaje(int personajeId, TipoConexion conexion = TipoConexion.ApiInvitado);
        CombateRegistro? ObtenerPorId(int id, TipoConexion conexion = TipoConexion.ApiInvitado);
        bool TieneTurnos(int id, TipoConexion conexion = TipoConexion.ApiInvitado);
        int Agregar(CombateRegistro combate);

        // Cambia participantes y reinicia su estado solo si el combate todavía no tiene turnos.
        bool ActualizarParticipantesSiNoTieneTurnos(CombateRegistro combate);

        // Elimina solo si el combate no tiene turnos.
        bool EliminarSiNoTieneTurnos(int id);
    }
}
