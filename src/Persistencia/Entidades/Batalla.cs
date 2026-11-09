using System;

namespace Persistencia.Entidades
{
    public class Batalla
    {
        private Personaje _combatiente1;
        private Personaje _combatiente2;
        private int _numeroTurno;
        public int Id { get; set; }

        public Personaje Combatiente1
        {
            get => _combatiente1;
            set => _combatiente1 = value ?? throw new ArgumentNullException(nameof(value), "El combatiente 1 no puede ser nulo.");
        }

        public Personaje Combatiente2
        {
            get => _combatiente2;
            set => _combatiente2 = value ?? throw new ArgumentNullException(nameof(value), "El combatiente 2 no puede ser nulo.");
        }

        public int NumeroTurno
        {
            get => _numeroTurno;
            set => _numeroTurno = value < 1 
                ? throw new ArgumentException("El número de turno debe ser mayor o igual a 1.") 
                : value;
        }

        public bool EsFinalizada => !Combatiente1.EstaVivo || !Combatiente2.EstaVivo;

        public bool EsTurnoDeCombatiente1 { get; set; } = true;
        public Personaje AtacanteActual => EsTurnoDeCombatiente1 ? Combatiente1 : Combatiente2;
        public Personaje DefensorActual => EsTurnoDeCombatiente1 ? Combatiente2 : Combatiente1;

        public Personaje? Ganador => !EsFinalizada
            ? null
            : (Combatiente1.EstaVivo ? Combatiente1 : (Combatiente2.EstaVivo ? Combatiente2 : null));

        public Batalla(Personaje combatiente1, Personaje combatiente2)
        {
            Combatiente1 = combatiente1;
            Combatiente2 = combatiente2;

            // Mismo objeto, o dos instancias del mismo personaje ya guardado (mismo Id).
            if (ReferenceEquals(Combatiente1, Combatiente2)
                || (Combatiente1.Id > 0 && Combatiente1.Id == Combatiente2.Id))
                throw new InvalidOperationException("Un personaje no puede luchar contra sí mismo.");

            NumeroTurno = 1;
        }

        public void EjecutarAtaque(Personaje atacante)
            => EjecutarAccion(atacante, defensor => atacante.Atacar(defensor));

        public void EjecutarHabilidad(Personaje atacante)
            => EjecutarAccion(atacante, defensor => atacante.UsarHabilidad(defensor));

        // Turnos alternados: solo actúa el atacante actual y, si el combate sigue, pasa el turno.
        private void EjecutarAccion(Personaje atacante, Action<Personaje> accion)
        {
            if (atacante == null) throw new ArgumentNullException(nameof(atacante));
            if (EsFinalizada) throw new InvalidOperationException("La batalla ya ha finalizado.");
            if (!ReferenceEquals(atacante, AtacanteActual))
                throw new InvalidOperationException($"No es el turno de {atacante.Nombre}.");

            accion(DefensorActual);

            if (!EsFinalizada)
                EsTurnoDeCombatiente1 = !EsTurnoDeCombatiente1;
        }
    }
}