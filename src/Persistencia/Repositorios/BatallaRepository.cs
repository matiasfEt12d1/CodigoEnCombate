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
            const string sql = @"
                INSERT INTO Batallas (combatiente1_id, combatiente2_id, numero_turno, es_finalizada)
                VALUES (@Combatiente1Id, @Combatiente2Id, @NumeroTurno, @EsFinalizada);
                SELECT LAST_INSERT_ID();";

            var c1Id = _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre ORDER BY id DESC LIMIT 1", new { Nombre = entidad.Combatiente1.Nombre });
            var c2Id = _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre ORDER BY id DESC LIMIT 1", new { Nombre = entidad.Combatiente2.Nombre });

            int idGenerado = _context.QueryFirst<int>(sql, new
            {
                Combatiente1Id = c1Id,
                Combatiente2Id = c2Id,
                entidad.NumeroTurno,
                entidad.EsFinalizada
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

        public void RegistrarHistorialTurno(int batallaId, int numeroTurno, int atacanteId, int defensorId, string? habilidadUsada, int danoCausado)
{
            const string sql = @"
                INSERT INTO historialbatallas (batalla_id, numero_turno, atacante_id, defensor_id, habilidad_usada, dano_causado, fecha_registro)
                VALUES (@BatallaId, @NumeroTurno, @AtacanteId, @DefensorId, @HabilidadUsada, @DanoCausado, NOW())";

            _context.Execute(sql, new
            {
                BatallaId = batallaId,
                NumeroTurno = numeroTurno,
                AtacanteId = atacanteId,
                DefensorId = defensorId,
                HabilidadUsada = habilidadUsada,
                DanoCausado = danoCausado
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