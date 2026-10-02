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

            var c1Id = _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre", new { Nombre = entidad.Combatiente1.Nombre });
            var c2Id = _context.QueryFirst<int>("SELECT id FROM Personajes WHERE nombre = @Nombre", new { Nombre = entidad.Combatiente2.Nombre });

            return _context.QueryFirst<int>(sql, new
            {
                Combatiente1Id = c1Id,
                Combatiente2Id = c2Id,
                entidad.NumeroTurno,
                entidad.EsFinalizada
            });
        }

        public bool Actualizar(Batalla entidad)
        {
            const string sql = "UPDATE Batallas SET numero_turno = @NumeroTurno, es_finalizada = @EsFinalizada WHERE id = @Id";
            int id = _context.QueryFirst<int>(
                "SELECT id FROM Batallas WHERE combatiente1_id = (SELECT id FROM Personajes WHERE nombre = @c1) AND combatiente2_id = (SELECT id FROM Personajes WHERE nombre = @c2) AND es_finalizada = 0",
                new { c1 = entidad.Combatiente1.Nombre, c2 = entidad.Combatiente2.Nombre }
            );
            return _context.Execute(sql, new { Id = id, entidad.NumeroTurno, entidad.EsFinalizada }) > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = "DELETE FROM Batallas WHERE id = @Id";
            return _context.Execute(sql, new { Id = id }) > 0;
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