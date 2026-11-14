using System;
using System.Collections.Generic;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public interface IBatallaRepository : IRepository<Batalla>
    {
        IEnumerable<Batalla> ObtenerBatallasActivas();
        bool FinalizarBatalla(int batallaId, int? ganadorId, int numeroTurno, int vidaC1, int vidaC2);
        void ProcesarAccionCombate(int batallaId, int atacanteId, int defensorId, int danoRealizado, int nuevaVidaDefensor, int? habilidadId, int numeroTurno);
        void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, int? habilidadId, int danoCausado, int nuevaVidaDefensor);

        // En una sola transacción (sp_RegistrarTurno): historial de las acciones, estado de los
        // dos combatientes y avance o cierre de la batalla. accionCombatiente2 es null si no llegó a actuar.
        void RegistrarTurno(Batalla batalla, int numeroTurno, AccionTurno accionCombatiente1, AccionTurno? accionCombatiente2);
    }

    // Acción de un combatiente dentro de un turno. HabilidadId es null en un ataque básico.
    public record AccionTurno(int? HabilidadId, int Danio);
}
