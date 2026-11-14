using Persistencia.Entidades;

namespace Aplicacion.Interfaces
{
    // Reglas de un turno sin base de datos: quién actúa, cuánto daño hace y si la batalla termina.
    public interface IResolucionTurnoService
    {
        TurnoResuelto ResolverTurno(
            Batalla batalla,
            AccionCombate accionCombatiente1 = AccionCombate.Atacar,
            AccionCombate accionCombatiente2 = AccionCombate.Atacar);
    }
}
