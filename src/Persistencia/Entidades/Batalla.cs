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
    }
}