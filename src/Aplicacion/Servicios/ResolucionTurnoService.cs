using Aplicacion.Interfaces;
using Persistencia.Entidades;

namespace Aplicacion.Servicios
{
    // Resuelve un turno sin tocar la base de datos: primero actúa el combatiente 1 y,
    // si el combatiente 2 sigue vivo, responde con su propia acción.
    // Qué hace cada acción lo decide cada personaje (polimorfismo).
    public class ResolucionTurnoService : IResolucionTurnoService
    {
        private sealed record Accion(
            Action<Personaje, Personaje> Ejecutar,
            Func<Personaje, bool> Disponible,
            Func<Personaje, int?> HabilidadUsada);

        private static readonly Dictionary<AccionCombate, Accion> Acciones = new()
        {
            // Un ataque básico siempre está disponible y no usa ninguna habilidad.
            [AccionCombate.Atacar] = new Accion(
                (atacante, defensor) => atacante.Atacar(defensor),
                _ => true,
                _ => null),

            // Depende de los recursos del personaje; se registra su habilidad guardada, si tiene una.
            [AccionCombate.Habilidad] = new Accion(
                (atacante, defensor) => atacante.UsarHabilidad(defensor),
                atacante => atacante.PuedeUsarHabilidad,
                atacante => atacante.Habilidades.FirstOrDefault(h => h.Id > 0)?.Id)
        };

        public TurnoResuelto ResolverTurno(
            Batalla batalla,
            AccionCombate accionCombatiente1 = AccionCombate.Atacar,
            AccionCombate accionCombatiente2 = AccionCombate.Atacar)
        {
            if (batalla == null) throw new ArgumentNullException(nameof(batalla));
            if (batalla.EsFinalizada) throw new InvalidOperationException("La batalla ya ha finalizado.");

            var accion1 = Buscar(accionCombatiente1);
            var accion2 = Buscar(accionCombatiente2);

            // Se revisa antes de actuar para que el turno no quede aplicado a medias.
            ValidarDisponible(batalla.Combatiente1, accionCombatiente1, accion1);
            ValidarDisponible(batalla.Combatiente2, accionCombatiente2, accion2);

            int numeroTurno = batalla.NumeroTurno;

            var resueltas = new List<AccionResuelta>
            {
                Resolver(batalla.Combatiente1, batalla.Combatiente2, accionCombatiente1, accion1)
            };

            if (!batalla.EsFinalizada)
                resueltas.Add(Resolver(batalla.Combatiente2, batalla.Combatiente1, accionCombatiente2, accion2));

            if (!batalla.EsFinalizada)
                batalla.NumeroTurno++;

            return new TurnoResuelto(numeroTurno, resueltas, batalla.EsFinalizada, batalla.Ganador);
        }

        private static Accion Buscar(AccionCombate accion) =>
            Acciones.TryGetValue(accion, out var encontrada)
                ? encontrada
                : throw new ArgumentOutOfRangeException(nameof(accion), $"Acción no soportada: {accion}");

        private static void ValidarDisponible(Personaje personaje, AccionCombate tipo, Accion accion)
        {
            if (!accion.Disponible(personaje))
                throw new InvalidOperationException($"{personaje.Nombre} no puede usar la acción '{tipo}': recursos insuficientes.");
        }

        private static AccionResuelta Resolver(Personaje atacante, Personaje defensor, AccionCombate tipo, Accion accion)
        {
            int antes = defensor.VidaEfectiva;
            accion.Ejecutar(atacante, defensor);
            int danio = Math.Max(0, antes - defensor.VidaEfectiva);

            return new AccionResuelta(atacante, defensor, tipo, accion.HabilidadUsada(atacante), danio);
        }
    }
}
