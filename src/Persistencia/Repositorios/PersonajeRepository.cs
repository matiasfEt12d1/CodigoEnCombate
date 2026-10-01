using Persistencia.Entidades;

namespace Persistencia.Repositorios
{
    public class PersonajeRepository : IPersonajeRepository
    {
        private readonly IDapperContext _context;

        public PersonajeRepository(IDapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Personaje? ObtenerPorId(int id)
        {
            const string sqlPersonaje = @"
                SELECT id AS Id, nombre AS Nombre, tipo AS Tipo, vida_max AS VidaMax, 
                       vida AS Vida, ataque AS Ataque, defensa AS Defensa, escudo AS Escudo, 
                       mana_max AS ManaMax, mana AS Mana, cantidad_flechas AS CantidadFlechas, 
                       probabilidad_critico AS ProbabilidadCritico 
                FROM Personajes WHERE id = @Id";

            var dto = _context.Query<PersonajeDto>(sqlPersonaje, new { Id = id }).FirstOrDefault();
            if (dto == null) return null;

            var personaje = MapearEntidad(dto);

            const string sqlHabilidades = @"
                SELECT nombre AS Nombre, costo_recurso AS CostoRecurso, potencia AS Potencia 
                FROM Habilidades WHERE personaje_id = @Id";

            var habilidades = _context.Query<HabilidadDto>(sqlHabilidades, new { Id = id });
            foreach (var h in habilidades)
            {
                personaje.AgregarHabilidad(new Habilidad(h.Nombre, h.CostoRecurso, h.Potencia));
            }

            return personaje;
        }

        public Personaje? ObtenerPorNombre(string nombre)
        {
            const string sql = "SELECT id FROM Personajes WHERE nombre = @Nombre";
            var id = _context.Query<int?>(sql, new { Nombre = nombre }).FirstOrDefault();
            return id.HasValue ? ObtenerPorId(id.Value) : null;
        }

        public IEnumerable<Personaje> ObtenerTodos()
        {
            const string sql = "SELECT id FROM Personajes";
            var ids = _context.Query<int>(sql);
            return ids.Select(ObtenerPorId).Where(p => p != null)!;
        }

        public IEnumerable<Personaje> ObtenerPorTipo(string tipo)
        {
            const string sql = "SELECT id FROM Personajes WHERE tipo = @Tipo";
            var ids = _context.Query<int>(sql, new { Tipo = tipo });
            return ids.Select(ObtenerPorId).Where(p => p != null)!;
        }

        public int Agregar(Personaje entidad)
        {
            const string sql = @"
                INSERT INTO Personajes (nombre, tipo, vida_max, vida, ataque, defensa, escudo, mana_max, mana, cantidad_flechas, probabilidad_critico)
                VALUES (@Nombre, @Tipo, @VidaMax, @Vida, @Ataque, @Defensa, @Escudo, @ManaMax, @Mana, @CantidadFlechas, @ProbabilidadCritico);
                SELECT LAST_INSERT_ID();";

            var pms = ObtenerParametros(entidad);
            int idGenerado = _context.QueryFirst<int>(sql, pms);

            foreach (var hab in entidad.Habilidades)
            {
                AgregarHabilidad(idGenerado, hab);
            }

            return idGenerado;
        }

        public bool Actualizar(Personaje entidad)
        {
            const string sql = @"
                UPDATE Personajes 
                SET vida = @Vida, ataque = @Ataque, defensa = @Defensa, escudo = @Escudo, mana = @Mana, cantidad_flechas = @CantidadFlechas
                WHERE nombre = @Nombre";

            var pms = ObtenerParametros(entidad);
            return _context.Execute(sql, pms) > 0;
        }

        public bool Eliminar(int id)
        {
            const string sql = "DELETE FROM Personajes WHERE id = @Id";
            return _context.Execute(sql, new { Id = id }) > 0;
        }

        public bool AgregarHabilidad(int personajeId, Habilidad habilidad)
        {
            const string sql = @"
                INSERT INTO Habilidades (personaje_id, nombre, costo_recurso, potencia)
                VALUES (@PersonajeId, @Nombre, @CostoRecurso, @Potencia)";

            return _context.Execute(sql, new
            {
                PersonajeId = personajeId,
                habilidad.Nombre,
                habilidad.CostoRecurso,
                habilidad.Potencia
            }) > 0;
        }

        private static Personaje MapearEntidad(PersonajeDto dto)
        {
            Personaje p = dto.Tipo switch
            {
                "Guerrero" => new Guerrero(dto.Nombre, dto.VidaMax, dto.Ataque, dto.Defensa, dto.Escudo),
                "Mago" => new Mago(dto.Nombre, dto.VidaMax, dto.Ataque, dto.Defensa, dto.ManaMax) { Mana = dto.Mana },
                "Arquero" => new Arquero(dto.Nombre, dto.VidaMax, dto.Ataque, dto.Defensa, dto.CantidadFlechas),
                "Asesino" => new Asesino(dto.Nombre, dto.VidaMax, dto.Ataque, dto.Defensa, dto.ProbabilidadCritico),
                _ => throw new InvalidOperationException($"Tipo de personaje desconocido: {dto.Tipo}")
            };
            p.Vida = dto.Vida;
            return p;
        }

        private static object ObtenerParametros(Personaje p)
        {
            return p switch
            {
                Guerrero g => new { g.Nombre, Tipo = "Guerrero", g.VidaMax, g.Vida, g.Ataque, g.Defensa, g.Escudo, ManaMax = 0, Mana = 0, CantidadFlechas = 0, ProbabilidadCritico = 0.0 },
                Mago m => new { m.Nombre, Tipo = "Mago", m.VidaMax, m.Vida, m.Ataque, m.Defensa, Escudo = 0, m.ManaMax, m.Mana, CantidadFlechas = 0, ProbabilidadCritico = 0.0 },
                Arquero a => new { a.Nombre, Tipo = "Arquero", a.VidaMax, a.Vida, a.Ataque, a.Defensa, Escudo = 0, ManaMax = 0, Mana = 0, a.CantidadFlechas, ProbabilidadCritico = 0.0 },
                Asesino s => new { s.Nombre, Tipo = "Asesino", s.VidaMax, s.Vida, s.Ataque, s.Defensa, Escudo = 0, ManaMax = 0, Mana = 0, CantidadFlechas = 0, s.ProbabilidadCritico },
                _ => throw new InvalidOperationException("Tipo no soportado")
            };
        }

        private class PersonajeDto
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Tipo { get; set; } = string.Empty;
            public int VidaMax { get; set; }
            public int Vida { get; set; }
            public int Ataque { get; set; }
            public int Defensa { get; set; }
            public int Escudo { get; set; }
            public int ManaMax { get; set; }
            public int Mana { get; set; }
            public int CantidadFlechas { get; set; }
            public double ProbabilidadCritico { get; set; }
        }

        private class HabilidadDto
        {
            public string Nombre { get; set; } = string.Empty;
            public int CostoRecurso { get; set; }
            public int Potencia { get; set; }
        }
    }
}