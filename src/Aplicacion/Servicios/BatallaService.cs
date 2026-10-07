using System;
using System.Collections.Generic;
using System.Linq;
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

            var p1 = _personajeRepository.ObtenerPorNombre(batalla.Combatiente1.Nombre) 
                    ?? throw new InvalidOperationException($"No se encontró al personaje '{batalla.Combatiente1.Nombre}' en la BD.");
            var p2 = _personajeRepository.ObtenerPorNombre(batalla.Combatiente2.Nombre) 
                    ?? throw new InvalidOperationException($"No se encontró al personaje '{batalla.Combatiente2.Nombre}' en la BD.");

            string habilidadP1 = batalla.Combatiente1.Habilidades.Count > 0 
                ? batalla.Combatiente1.Habilidades[0].Nombre 
                : "Ataque Básico";

            // 1. Ataque Combatiente 1 -> Combatiente 2
            int vidaAntesP2 = batalla.Combatiente2.Vida;
            batalla.Combatiente1.Atacar(batalla.Combatiente2);
            int danoP1 = Math.Max(0, vidaAntesP2 - batalla.Combatiente2.Vida);

            _batallaRepository.ProcesarAccionCombate(
                batalla.Id, 
                p1.Id, 
                p2.Id, 
                danoP1, 
                batalla.Combatiente2.Vida, 
                habilidadP1, 
                batalla.NumeroTurno
            );

            // 2. Contraataque Combatiente 2 -> Combatiente 1 (si sigue vivo)
            if (batalla.Combatiente2.EstaVivo)
            {
                string habilidadP2 = batalla.Combatiente2.Habilidades.Count > 0 
                    ? batalla.Combatiente2.Habilidades[0].Nombre 
                    : "Ataque Básico";

                int vidaAntesP1 = batalla.Combatiente1.Vida;
                batalla.Combatiente2.Atacar(batalla.Combatiente1);
                int danoP2 = Math.Max(0, vidaAntesP1 - batalla.Combatiente1.Vida);

                _batallaRepository.ProcesarAccionCombate(
                    batalla.Id, 
                    p2.Id, 
                    p1.Id, 
                    danoP2, 
                    batalla.Combatiente1.Vida, 
                    habilidadP2, 
                    batalla.NumeroTurno
                );
            }

            // Actualizar recursos de los personajes en la BD (maná, escudo, etc.)
            _personajeRepository.Actualizar(batalla.Combatiente1);
            _personajeRepository.Actualizar(batalla.Combatiente2);

            // Si un combatiente muere, EsFinalizada se evalúa automáticamente como true en el modelo de Dominio
            if (batalla.EsFinalizada)
            {
                int? ganadorId = batalla.Combatiente1.EstaVivo ? p1.Id : (batalla.Combatiente2.EstaVivo ? p2.Id : null);
                _batallaRepository.FinalizarBatalla(batalla.Id, ganadorId, batalla.NumeroTurno, batalla.Combatiente1.Vida, batalla.Combatiente2.Vida);
            }
            else
            {
                batalla.NumeroTurno++;
                _batallaRepository.Actualizar(batalla);
            }

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