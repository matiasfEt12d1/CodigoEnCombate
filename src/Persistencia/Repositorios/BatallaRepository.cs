using System;
using System.Collections.Generic;
using System.Linq;
using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class BatallaRepository : IBatallaRepository
    {
        private readonly IDapperContext _context;
        private readonly IPersonajeRepository _personajeRepository;

        public BatallaRepository(IDapperContext context, IPersonajeRepository personajeRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _personajeRepository = personajeRepository ?? throw new ArgumentNullException(nameof(personajeRepository));
        }

        public Batalla? ObtenerPorId(int id)
        {
            const string sql = @"
                SELECT id AS Id, combatiente1_id AS Combatiente1Id, combatiente2_id AS Combatiente2Id, 
                       numero_turno AS NumeroTurno, es_finalizada AS EsFinalizada 
                FROM Batallas WHERE id = @Id";

            var dto = _context.Query<BatallaDto>(sql, new { Id = id }).FirstOrDefault();
            if (dto == null) return null;

            var c1 = _personajeRepository.ObtenerPorId(dto.Combatiente1Id);
            var c2 = _personajeRepository.ObtenerPorId(dto.Combatiente2Id);

            if (c1 == null || c2 == null) return null;

            return new Batalla(c1, c2)
            {
                Id = dto.Id,
                NumeroTurno = dto.NumeroTurno
            };
        }

        public IEnumerable<Batalla> ObtenerTodos()
        {
            const string sql = "SELECT id FROM Batallas";
            var ids = _context.Query<int>(sql);
            return ids.Select(ObtenerPorId).Where(b => b != null)!;
        }

        public IEnumerable<Batalla> ObtenerBatallasActivas()
        {
            const string sql = "SELECT id FROM Batallas WHERE es_finalizada = 0";
            var ids = _context.Query<int>(sql);
            return ids.Select(ObtenerPorId).Where(b => b != null)!;
        }

        public int Agregar(Batalla entidad)
        {
            if (entidad == null) throw new ArgumentNullException(nameof(entidad));

            // Por ID: buscar por nombre podría tomar a otro personaje con el mismo nombre.
            if (entidad.Combatiente1.Id <= 0 || entidad.Combatiente2.Id <= 0)
                throw new InvalidOperationException("Los combatientes deben estar registrados antes de guardar la batalla.");

            const string sql = "CALL sp_RegistrarBatalla(@p_combatiente1_id, @p_combatiente2_id);";

            int idGenerado = _context.QueryFirst<int>(sql, new
            {
                p_combatiente1_id = entidad.Combatiente1.Id,
                p_combatiente2_id = entidad.Combatiente2.Id
            });

            entidad.Id = idGenerado;
            return idGenerado;
        }

        public bool Actualizar(Batalla entidad)
        {
            const string sql = @"
                UPDATE Batallas 
                SET numero_turno = @NumeroTurno, 
                    es_finalizada = @EsFinalizada 
                WHERE id = @Id";

            return _context.Execute(sql, new 
            { 
                Id = entidad.Id,
                entidad.NumeroTurno, 
                entidad.EsFinalizada 
            }) > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = "DELETE FROM Batallas WHERE id = @Id";
            return _context.Execute(sql, new { Id = id }) > 0;
        }

        public bool FinalizarBatalla(int batallaId, int? ganadorId, int numeroTurno, int vidaC1, int vidaC2)
        {
            const string sql = "CALL sp_FinalizarBatalla(@p_batalla_id, @p_ganador_id, @p_numero_turno, @p_vida_c1, @p_vida_c2);";

            return _context.Execute(sql, new
            {
                p_batalla_id = batallaId,
                p_ganador_id = ganadorId,
                p_numero_turno = numeroTurno,
                p_vida_c1 = vidaC1,
                p_vida_c2 = vidaC2
            }) > 0;
        }

        public void ProcesarAccionCombate(int batallaId, int atacanteId, int defensorId, int danoRealizado, int nuevaVidaDefensor, int? habilidadId, int numeroTurno)
        {
            const string sql = @"CALL sp_ProcesarAccionCombate(
                @p_batalla_id,
                @p_atacante_id,
                @p_defensor_id,
                @p_dano_realizado,
                @p_nueva_vida_defensor,
                @p_habilidad_id,
                @p_numero_turno
            );";

            _context.Execute(sql, new
            {
                p_batalla_id = batallaId,
                p_atacante_id = atacanteId,
                p_defensor_id = defensorId,
                p_dano_realizado = danoRealizado,
                p_nueva_vida_defensor = nuevaVidaDefensor,
                p_habilidad_id = habilidadId,
                p_numero_turno = numeroTurno
            });
        }

        public void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, int? habilidadId, int danoCausado, int nuevaVidaDefensor)
        {
            ProcesarAccionCombate(batallaId, atacanteId, defensorId, danoCausado, nuevaVidaDefensor, habilidadId, numeroTurno);
        }

        public void RegistrarTurno(Batalla batalla, int numeroTurno, AccionTurno accionCombatiente1, AccionTurno? accionCombatiente2)
        {
            if (batalla == null) throw new ArgumentNullException(nameof(batalla));
            if (accionCombatiente1 == null) throw new ArgumentNullException(nameof(accionCombatiente1));

            var c1 = PersonajeRepository.ObtenerParametros(batalla.Combatiente1);
            var c2 = PersonajeRepository.ObtenerParametros(batalla.Combatiente2);

            const string sql = @"CALL sp_RegistrarTurno(
                @p_batalla_id, @p_numero_turno,
                @p_dano_c1, @p_habilidad_c1_id,
                @p_dano_c2, @p_habilidad_c2_id,
                @p_vida_c1, @p_escudo_c1, @p_mana_c1, @p_flechas_c1,
                @p_vida_c2, @p_escudo_c2, @p_mana_c2, @p_flechas_c2,
                @p_finalizada, @p_ganador_id
            );";

            _context.Execute(sql, new
            {
                p_batalla_id = batalla.Id,
                p_numero_turno = numeroTurno,
                p_dano_c1 = accionCombatiente1.Danio,
                p_habilidad_c1_id = accionCombatiente1.HabilidadId,
                p_dano_c2 = accionCombatiente2?.Danio,
                p_habilidad_c2_id = accionCombatiente2?.HabilidadId,
                p_vida_c1 = c1.Vida,
                p_escudo_c1 = c1.Escudo,
                p_mana_c1 = c1.Mana,
                p_flechas_c1 = c1.CantidadFlechas,
                p_vida_c2 = c2.Vida,
                p_escudo_c2 = c2.Escudo,
                p_mana_c2 = c2.Mana,
                p_flechas_c2 = c2.CantidadFlechas,
                p_finalizada = batalla.EsFinalizada,
                p_ganador_id = batalla.Ganador?.Id
            });
        }

        private class BatallaDto
        {
            public int Id { get; set; }
            public int Combatiente1Id { get; set; }
            public int Combatiente2Id { get; set; }
            public int NumeroTurno { get; set; }
            public bool EsFinalizada { get; set; }
        }
    }
}