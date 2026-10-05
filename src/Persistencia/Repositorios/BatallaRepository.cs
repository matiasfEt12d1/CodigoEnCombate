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
            int c1Id = entidad.Combatiente1.Id > 0 
                ? entidad.Combatiente1.Id 
                : _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre ORDER BY id DESC LIMIT 1", new { Nombre = entidad.Combatiente1.Nombre });

            int c2Id = entidad.Combatiente2.Id > 0 
                ? entidad.Combatiente2.Id 
                : _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre ORDER BY id DESC LIMIT 1", new { Nombre = entidad.Combatiente2.Nombre });

            const string sql = "CALL sp_RegistrarBatalla(@p_combatiente1_id, @p_combatiente2_id);";

            int idGenerado = _context.QueryFirst<int>(sql, new
            {
                p_combatiente1_id = c1Id,
                p_combatiente2_id = c2Id
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

        public void ProcesarAccionCombate(int batallaId, int atacanteId, int defensorId, int danoRealizado, int nuevaVidaDefensor, string? habilidadUsada, int numeroTurno)
        {
            const string sql = @"CALL sp_ProcesarAccionCombate(
                @p_batalla_id, 
                @p_atacante_id, 
                @p_defensor_id, 
                @p_dano_realizado, 
                @p_nueva_vida_defensor, 
                @p_habilidad_usada, 
                @p_numero_turno
            );";

            _context.Execute(sql, new
            {
                p_batalla_id = batallaId,
                p_atacante_id = atacanteId,
                p_defensor_id = defensorId,
                p_dano_realizado = danoRealizado,
                p_nueva_vida_defensor = nuevaVidaDefensor,
                p_habilidad_usada = habilidadUsada,
                p_numero_turno = numeroTurno
            });
            //TODO ¿Qué pasa si falla algo arriba?
        }

        public void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, string? habilidadUsada, int danoCausado, int nuevaVidaDefensor)
        {
            ProcesarAccionCombate(batallaId, atacanteId, defensorId, danoCausado, nuevaVidaDefensor, habilidadUsada, numeroTurno);
        }

        public IEnumerable<RankingTipoPersonajeDto> ObtenerRankingPorTipoPersonaje(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_ObtenerRankingPorTipoPersonaje(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<RankingTipoPersonajeDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
            });
        }

        public IEnumerable<ReporteBatallaDetalladoDto> GenerarReporteBatallasDetallado(DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = "CALL sp_GenerarReporteBatallasDetallado(@p_fecha_desde, @p_fecha_hasta);";

            return _context.Query<ReporteBatallaDetalladoDto>(sql, new
            {
                p_fecha_desde = fechaDesde,
                p_fecha_hasta = fechaHasta
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

    public class RankingTipoPersonajeDto
    {
        public string TipoPersonaje { get; set; } = string.Empty;
        public int TotalBatallas { get; set; }
        public int TotalVictorias { get; set; }
        public int TotalDerrotas { get; set; }
        public decimal DanoPromedioPorAccion { get; set; }
    }

    public class ReporteBatallaDetalladoDto
    {
        public int BatallaId { get; set; }
        public DateTime FechaInicio { get; set; }
        public int TurnosTotales { get; set; }
        public bool Finalizada { get; set; }
        public string Combatiente1_Nombre { get; set; } = string.Empty;
        public string Combatiente1_Tipo { get; set; } = string.Empty;
        public string Combatiente2_Nombre { get; set; } = string.Empty;
        public string Combatiente2_Tipo { get; set; } = string.Empty;
        public string Ganador_Nombre { get; set; } = string.Empty;
        public string Ganador_Tipo { get; set; } = string.Empty;
    }
}