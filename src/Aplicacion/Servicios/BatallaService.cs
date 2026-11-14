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
        private readonly IResolucionTurnoService _resolucionTurno;

        public BatallaService(
            IBatallaRepository batallaRepository,
            IPersonajeRepository personajeRepository,
            IResolucionTurnoService resolucionTurno)
        {
            _batallaRepository = batallaRepository ?? throw new ArgumentNullException(nameof(batallaRepository));
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
            _resolucionTurno = resolucionTurno ?? throw new ArgumentNullException(nameof(resolucionTurno));
        }

        public Batalla CrearBatalla(Personaje combatiente1, Personaje combatiente2)
        {
            if (combatiente1 == null || combatiente2 == null)
                throw new ArgumentNullException("Ambos combatientes deben ser válidos.");

            if (combatiente1.Id <= 0 || combatiente2.Id <= 0)
                throw new InvalidOperationException("Los combatientes deben estar registrados antes de iniciar una batalla.");

            var batalla = new Batalla(combatiente1, combatiente2);

            // Cada batalla empieza con los combatientes recuperados (sp_RegistrarBatalla hace lo mismo en la base),
            // así un personaje derrotado puede volver a pelear.
            combatiente1.Restaurar();
            combatiente2.Restaurar();

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

        // Resuelve el turno con las reglas del dominio y lo guarda entero en una sola transacción.
        public bool EjecutarTurno(
            Batalla batalla,
            AccionCombate accionCombatiente1 = AccionCombate.Atacar,
            AccionCombate accionCombatiente2 = AccionCombate.Atacar)
        {
            if (batalla == null) throw new ArgumentNullException(nameof(batalla));
            if (batalla.EsFinalizada)
                throw new InvalidOperationException("La batalla ya ha finalizado.");
            if (batalla.Id <= 0)
                throw new InvalidOperationException("La batalla debe estar registrada antes de ejecutar turnos.");
            if (batalla.Combatiente1.Id <= 0 || batalla.Combatiente2.Id <= 0)
                throw new InvalidOperationException("Los combatientes deben estar registrados antes de ejecutar turnos.");

            var turno = _resolucionTurno.ResolverTurno(batalla, accionCombatiente1, accionCombatiente2);

            var accion1 = turno.Acciones[0];
            var accion2 = turno.Acciones.Count > 1 ? turno.Acciones[1] : null;

            _batallaRepository.RegistrarTurno(
                batalla,
                turno.NumeroTurno,
                new AccionTurno(accion1.HabilidadId, accion1.Danio),
                accion2 == null ? null : new AccionTurno(accion2.HabilidadId, accion2.Danio));

            return !turno.Finalizada;
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