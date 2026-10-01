using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class HabilidadRepository : IHabilidadRepository
    {
        private readonly IDapperContext _context;

        public HabilidadRepository(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Habilidad? ObtenerPorId(int id)
        {
            const string sql = "SELECT nombre AS Nombre, costo_recurso AS CostoRecurso, potencia AS Potencia FROM Habilidades WHERE id = @Id";
            var dto = _context.Query<HabilidadDto>(sql, new { Id = id }).FirstOrDefault();
            return dto == null ? null : new Habilidad(dto.Nombre, dto.CostoRecurso, dto.Potencia);
        }

        public IEnumerable<Habilidad> ObtenerTodos()
        {
            const string sql = "SELECT nombre AS Nombre, costo_recurso AS CostoRecurso, potencia AS Potencia FROM Habilidades";
            return _context.Query<HabilidadDto>(sql).Select(d => new Habilidad(d.Nombre, d.CostoRecurso, d.Potencia));
        }

        public IEnumerable<Habilidad> ObtenerPorPersonajeId(int personajeId)
        {
            const string sql = "SELECT nombre AS Nombre, costo_recurso AS CostoRecurso, potencia AS Potencia FROM Habilidades WHERE personaje_id = @PersonajeId";
            return _context.Query<HabilidadDto>(sql, new { PersonajeId = personajeId }).Select(d => new Habilidad(d.Nombre, d.CostoRecurso, d.Potencia));
        }

        public int Agregar(Habilidad entidad)
        {
            const string sql = @"
                INSERT INTO Habilidades (personaje_id, nombre, costo_recurso, potencia)
                VALUES (1, @Nombre, @CostoRecurso, @Potencia);
                SELECT LAST_INSERT_ID();";

            return _context.QueryFirst<int>(sql, entidad);
        }

        public bool Actualizar(Habilidad entidad)
        {
            const string sql = "UPDATE Habilidades SET costo_recurso = @CostoRecurso, potencia = @Potencia WHERE nombre = @Nombre";
            return _context.Execute(sql, entidad) > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = "DELETE FROM Habilidades WHERE id = @Id";
            return _context.Execute(sql, new { Id = id }) > 0;
        }

        private class HabilidadDto
        {
            public string Nombre { get; set; } = string.Empty;
            public int CostoRecurso { get; set; }
            public int Potencia { get; set; }
        }
    }
}