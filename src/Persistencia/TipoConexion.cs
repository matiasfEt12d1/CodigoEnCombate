namespace Persistencia;

// Cada conexión se configura con la variable de entorno ConnectionStrings__<nombre>.
public enum TipoConexion
{
    // Simulador del bimestre anterior (tablas del simulador en combate_api).
    Desarrollo,
    Administrador,

    // Combate por API (base combate_api): lecturas con combate_invitado y escrituras con combate_admin.
    ApiInvitado,
    ApiAdministrador
}
