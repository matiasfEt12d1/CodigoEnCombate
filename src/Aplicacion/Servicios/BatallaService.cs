using Aplicacion.Interfaces;
using Persistencia.Entidades;
using Persistencia.Repositorios;

namespace Aplicacion.Servicios
{
    public class BatallaService : IBatallaService
    {
        private readonly IBatallaRepository _batallaRepository;
        private readonly IPersonajeRepository _personajeRepository;

        public BatallaService(IBatallaRepository batallaRepository, IPersonajeRepository personajeRepository)
        {
            _batallaRepository = batallaRepository ?? throw new ArgumentNullException(nameof(batallaRepository));
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
        }

        public Batalla CrearBatalla(Personaje combatiente1, Personaje combatiente2)
        {
            if (combatiente1 == null || combatiente2 == null)
                throw new ArgumentNullException("Ambos combatientes deben ser válidos.");

            if (!combatiente1.EstaVivo || !combatiente2.EstaVivo)
                throw new InvalidOperationException("Ambos personajes deben estar vivos para iniciar un combate.");

            var batalla = new Batalla(combatiente1, combatiente2);
            _batallaRepository.Agregar(batalla);
            return batalla;
        }

        public Batalla CrearBatallaPorIds(int combatiente1Id, int combatiente2Id)
        {
            if (combatiente1Id == combatiente2Id)
                throw new InvalidOperationException("Un personaje no puede luchar contra sí mismo.");

            var p1 = _personajeRepository.ObtenerPorId(combatiente1Id)
                ?? throw new KeyNotFoundException($"No se encontró el personaje con ID {combatiente1Id}.");
            var p2 = _personajeRepository.ObtenerPorId(combatiente2Id)
                ?? throw new KeyNotFoundException($"No se encontró el personaje con ID {combatiente2Id}.");

            return CrearBatalla(p1, p2);
        }

        public bool EjecutarTurno(Batalla batalla)
        {
            if (batalla == null) throw new ArgumentNullException(nameof(batalla));
            if (batalla.EsFinalizada)
                throw new InvalidOperationException("La batalla ya ha finalizado.");

            // Turno Combatiente 1 (ejecución polimórfica según la subclase)
            batalla.Combatiente1.Atacar(batalla.Combatiente2);

            // Si el combatiente 2 sigue con vida, contraataca
            if (batalla.Combatiente2.EstaVivo)
            {
                batalla.Combatiente2.Atacar(batalla.Combatiente1);
            }

            batalla.NumeroTurno++;
            _batallaRepository.Actualizar(batalla);

            return !batalla.EsFinalizada;
        }

        public Personaje EjecutarCombateCompleto(Batalla batalla)
        {
            if (batalla == null) throw new ArgumentNullException(nameof(batalla));

            while (!batalla.EsFinalizada)
            {
                EjecutarTurno(batalla);
            }

            return batalla.Combatiente1.EstaVivo ? batalla.Combatiente1 : batalla.Combatiente2;
        }

        public Batalla? BuscarPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("El ID de la batalla debe ser mayor a cero.", nameof(id));
            return _batallaRepository.ObtenerPorId(id);
        }

        public IEnumerable<Batalla> ListarBatallas()
        {
            return _batallaRepository.ObtenerTodos();
        }

        public IEnumerable<Batalla> ListarBatallasActivas()
        {
            return _batallaRepository.ObtenerBatallasActivas();
        }

        public IEnumerable<Batalla> ObtenerHistorialCombates()
        {
            return _batallaRepository.ObtenerTodos().Where(b => b.EsFinalizada);
        }
    }
}